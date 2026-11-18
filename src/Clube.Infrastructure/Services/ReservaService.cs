using Clube.Application.Interfaces;
using Clube.Domain.Entities;
using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Clube.Infrastructure.Services;

public class ReservaService : IReservaService
{
    private readonly ClubeDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ReservaService(ClubeDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<EspacoDto>> ListarEspacosAsync(Guid? clubeId = null, CancellationToken ct = default)
    {
        var query = _db.Espacos.AsQueryable();
        if (clubeId.HasValue)
        {
            query = query.Where(e => e.ClubeId == clubeId.Value);
        }

        var espacos = await query.OrderBy(e => e.Nome).ToListAsync(ct);
        return espacos.Select(e => e.ToDto()).ToList();
    }

    public async Task<EspacoDto> CriarEspacoAsync(CriarEspacoRequest request, CancellationToken ct = default)
    {
        var clubeId = _tenantContext.CurrentClubeId ?? request.ClubeId;
        var espaco = new Espaco
        {
            Id = Guid.NewGuid(),
            ClubeId = clubeId,
            Nome = request.Nome.Trim(),
            Capacidade = Math.Max(1, request.Capacidade),
            Descricao = request.Descricao.Trim(),
            TaxaReserva = request.TaxaReserva,
            Ativo = true
        };

        _db.Espacos.Add(espaco);
        await _db.SaveChangesAsync(ct);
        return espaco.ToDto();
    }

    public async Task<List<ReservaEspacoDto>> ListarReservasAsync(Guid? clubeId = null, CancellationToken ct = default)
    {
        var query = _db.ReservasEspacos
            .Include(r => r.Espaco)
            .Include(r => r.Associado)
            .AsQueryable();

        if (clubeId.HasValue)
        {
            query = query.Where(r => r.ClubeId == clubeId.Value);
        }

        var reservas = await query.OrderBy(r => r.DataHoraInicio).ToListAsync(ct);

        return reservas.Select(r =>
        {
            var temConflitoPendente = r.Status == StatusReserva.PendenteAprovacao &&
                reservas.Any(outra =>
                    (outra.Status == StatusReserva.PendenteAprovacao || outra.Status == StatusReserva.Confirmada) &&
                    r.ConflitaCom(outra));

            return r.ToDto(temConflitoPendente);
        }).ToList();
    }

    public async Task<ReservaEspacoDto> SolicitarReservaAsync(CriarReservaRequest request, CancellationToken ct = default)
    {
        if (request.DataHoraFim <= request.DataHoraInicio)
        {
            throw new ArgumentException("A data/hora final da reserva deve ser posterior à data/hora inicial.");
        }

        var espaco = await _db.Espacos.FirstOrDefaultAsync(e => e.Id == request.EspacoId, ct)
            ?? throw new InvalidOperationException("Espaço não encontrado para o clube atual.");

        var associado = await _db.Associados.FirstOrDefaultAsync(a => a.Id == request.AssociadoId, ct)
            ?? throw new InvalidOperationException("Associado não encontrado para o clube atual.");

        if (request.Id.HasValue)
        {
            var existente = await _db.ReservasEspacos
                .Include(r => r.Espaco)
                .Include(r => r.Associado)
                .FirstOrDefaultAsync(r => r.Id == request.Id.Value, ct);
            if (existente is not null)
            {
                if (request.CriadoOffline && existente.SincronizadoEm is null)
                {
                    existente.SincronizadoEm = DateTime.UtcNow;
                    await _db.SaveChangesAsync(ct);
                }
                return existente.ToDto();
            }
        }

        var concorrentes = await _db.ReservasEspacos
            .Where(r => r.EspacoId == espaco.Id &&
                        (r.Status == StatusReserva.Confirmada || r.Status == StatusReserva.PendenteAprovacao))
            .ToListAsync(ct);

        var sobrepostas = concorrentes
            .Where(r => r.SobrepoeHorario(request.DataHoraInicio, request.DataHoraFim))
            .ToList();

        var sobrepoeConfirmada = sobrepostas.Any(r => r.Status == StatusReserva.Confirmada);
        var sobrepoePendente = sobrepostas.Any(r => r.Status == StatusReserva.PendenteAprovacao);

        var observacoes = request.Observacoes?.Trim() ?? string.Empty;
        var statusInicial = StatusReserva.PendenteAprovacao;

        if (sobrepoeConfirmada)
        {
            statusInicial = StatusReserva.Conflito;
            observacoes = string.IsNullOrEmpty(observacoes)
                ? "[Conflito detectado com reserva já confirmada]"
                : $"{observacoes} [Conflito detectado com reserva já confirmada]";
        }

        var reserva = new ReservaEspaco
        {
            Id = request.Id ?? Guid.NewGuid(),
            EspacoId = espaco.Id,
            ClubeId = espaco.ClubeId,
            AssociadoId = associado.Id,
            FinalidadeUso = request.FinalidadeUso,
            DataHoraInicio = request.DataHoraInicio,
            DataHoraFim = request.DataHoraFim,
            Status = statusInicial,
            Observacoes = observacoes,
            CriadoOffline = request.CriadoOffline,
            SincronizadoEm = request.CriadoOffline ? DateTime.UtcNow : null
        };

        _db.ReservasEspacos.Add(reserva);
        await _db.SaveChangesAsync(ct);

        reserva.Espaco = espaco;
        reserva.Associado = associado;
        return reserva.ToDto(temConflitoPendente: !sobrepoeConfirmada && sobrepoePendente);
    }

    public async Task<ReservaEspacoDto?> AprovarReservaAsync(Guid reservaId, CancellationToken ct = default)
    {
        var reserva = await _db.ReservasEspacos
            .Include(r => r.Espaco)
            .Include(r => r.Associado)
            .FirstOrDefaultAsync(r => r.Id == reservaId, ct);

        if (reserva is null)
        {
            return null;
        }

        var candidatas = await _db.ReservasEspacos
            .Where(r => r.EspacoId == reserva.EspacoId &&
                        r.Id != reserva.Id &&
                        (r.Status == StatusReserva.Confirmada ||
                         r.Status == StatusReserva.PendenteAprovacao ||
                         r.Status == StatusReserva.Conflito))
            .ToListAsync(ct);

        var jaExisteConfirmadaConflitante = candidatas.Any(r =>
            r.Status == StatusReserva.Confirmada && r.ConflitaCom(reserva));

        if (jaExisteConfirmadaConflitante)
        {
            throw new InvalidOperationException("Já existe outra reserva confirmada sobrepondo este horário.");
        }

        reserva.Status = StatusReserva.Confirmada;

        foreach (var concorrente in candidatas.Where(r =>
                     (r.Status == StatusReserva.PendenteAprovacao || r.Status == StatusReserva.Conflito) &&
                     r.ConflitaCom(reserva)))
        {
            concorrente.Status = StatusReserva.RejeitadaPorConflito;
            concorrente.Observacoes = string.IsNullOrWhiteSpace(concorrente.Observacoes)
                ? "Rejeitada automaticamente por conflito na mediação presidencial."
                : $"{concorrente.Observacoes} [Rejeitada por conflito]";
        }

        await _db.SaveChangesAsync(ct);
        return reserva.ToDto();
    }

    public async Task<ReservaEspacoDto?> RejeitarReservaAsync(Guid reservaId, string? motivo = null, CancellationToken ct = default)
    {
        var reserva = await _db.ReservasEspacos
            .Include(r => r.Espaco)
            .Include(r => r.Associado)
            .FirstOrDefaultAsync(r => r.Id == reservaId, ct);

        if (reserva is null)
        {
            return null;
        }

        reserva.Status = StatusReserva.Rejeitada;
        if (!string.IsNullOrWhiteSpace(motivo))
        {
            reserva.Observacoes = string.IsNullOrWhiteSpace(reserva.Observacoes)
                ? motivo.Trim()
                : $"{reserva.Observacoes} | Motivo rejeição: {motivo.Trim()}";
        }

        await _db.SaveChangesAsync(ct);
        return reserva.ToDto();
    }
}
