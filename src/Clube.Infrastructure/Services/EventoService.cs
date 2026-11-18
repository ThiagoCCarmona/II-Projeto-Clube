using Clube.Application.Interfaces;
using Clube.Domain.Entities;
using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Clube.Infrastructure.Services;

public class EventoService : IEventoService
{
    private readonly ClubeDbContext _db;
    private readonly ITenantContext _tenantContext;

    public EventoService(ClubeDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<EventoDto>> ListarAsync(Guid? clubeId = null, CancellationToken ct = default)
    {
        var query = _db.Eventos.Include(e => e.Inscricoes).AsQueryable();
        if (clubeId.HasValue)
        {
            query = query.Where(e => e.ClubeId == clubeId.Value);
        }

        var itens = await query.OrderBy(e => e.DataInicio).ToListAsync(ct);
        return itens.Select(e => e.ToDto()).ToList();
    }

    public async Task<EventoDto> CriarAsync(CriarEventoRequest request, CancellationToken ct = default)
    {
        if (request.DataFim <= request.DataInicio)
        {
            throw new ArgumentException("A data/hora final do evento deve ser posterior à data/hora inicial.");
        }

        var clubeId = _tenantContext.CurrentClubeId ?? request.ClubeId;

        var evento = new Evento
        {
            Id = Guid.NewGuid(),
            ClubeId = clubeId,
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            TipoEvento = request.TipoEvento,
            PublicoAlvo = request.PublicoAlvo,
            DataInicio = request.DataInicio,
            DataFim = request.DataFim,
            Local = request.Local.Trim(),
            ValorIngresso = request.ValorIngresso,
            RequerInscricao = request.RequerInscricao,
            Ativo = true
        };

        _db.Eventos.Add(evento);
        await _db.SaveChangesAsync(ct);
        return evento.ToDto();
    }

    public async Task<InscricaoEventoDto> InscreverAsync(Guid eventoId, InscreverEventoRequest request, CancellationToken ct = default)
    {
        var evento = await _db.Eventos.FirstOrDefaultAsync(e => e.Id == eventoId, ct)
            ?? throw new InvalidOperationException("Evento não encontrado.");

        var associado = await _db.Associados.FirstOrDefaultAsync(a => a.Id == request.AssociadoId, ct)
            ?? throw new InvalidOperationException("Associado não encontrado.");

        if (!associado.PodeParticiparDoEvento(evento.PublicoAlvo))
        {
            throw new InvalidOperationException(
                $"Associado do tipo {associado.TipoAssociado} ({associado.StatusAprovacao}) não atende ao público-alvo {evento.PublicoAlvo} deste evento.");
        }

        var existente = await _db.InscricoesEventos
            .FirstOrDefaultAsync(i => i.EventoId == eventoId && i.AssociadoId == associado.Id, ct);

        if (existente is not null)
        {
            existente.QuantidadeAcompanhantes = Math.Max(0, request.QuantidadeAcompanhantes);
            existente.Status = StatusInscricao.Confirmada;
            existente.DataInscricao = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return new InscricaoEventoDto(
                existente.Id, evento.Id, evento.Titulo, associado.Id, associado.Nome,
                existente.DataInscricao, existente.QuantidadeAcompanhantes, existente.Status);
        }

        var inscricao = new InscricaoEvento
        {
            Id = Guid.NewGuid(),
            ClubeId = evento.ClubeId,
            EventoId = evento.Id,
            AssociadoId = associado.Id,
            DataInscricao = DateTime.UtcNow,
            QuantidadeAcompanhantes = Math.Max(0, request.QuantidadeAcompanhantes),
            Status = StatusInscricao.Confirmada
        };

        _db.InscricoesEventos.Add(inscricao);
        await _db.SaveChangesAsync(ct);

        return new InscricaoEventoDto(
            inscricao.Id, evento.Id, evento.Titulo, associado.Id, associado.Nome,
            inscricao.DataInscricao, inscricao.QuantidadeAcompanhantes, inscricao.Status);
    }

    public async Task<List<InscricaoEventoDto>> ListarInscricoesAsync(Guid eventoId, CancellationToken ct = default)
    {
        var itens = await _db.InscricoesEventos
            .Include(i => i.Evento)
            .Include(i => i.Associado)
            .Where(i => i.EventoId == eventoId)
            .OrderByDescending(i => i.DataInscricao)
            .ToListAsync(ct);

        return itens.Select(i => new InscricaoEventoDto(
            i.Id,
            i.EventoId,
            i.Evento?.Titulo ?? string.Empty,
            i.AssociadoId,
            i.Associado?.Nome ?? string.Empty,
            i.DataInscricao,
            i.QuantidadeAcompanhantes,
            i.Status)).ToList();
    }
}
