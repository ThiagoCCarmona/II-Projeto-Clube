using Clube.Application.Interfaces;
using Clube.Application.Payments;
using Clube.Domain.Entities;
using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Clube.Infrastructure.Services;

public class MensalidadeService : IMensalidadeService
{
    private readonly ClubeDbContext _db;
    private readonly ITenantContext _tenantContext;

    public MensalidadeService(ClubeDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<MensalidadeDto>> ListarAsync(Guid? clubeId = null, Guid? associadoId = null, CancellationToken ct = default)
    {
        var query = _db.Mensalidades
            .Include(m => m.Associado)
            .AsQueryable();

        if (clubeId.HasValue)
        {
            query = query.Where(m => m.ClubeId == clubeId.Value);
        }

        if (associadoId.HasValue)
        {
            query = query.Where(m => m.AssociadoId == associadoId.Value);
        }

        var itens = await query
            .OrderByDescending(m => m.DataVencimento)
            .ToListAsync(ct);

        return itens.Select(m => m.ToDto()).ToList();
    }

    public async Task<GerarLoteMensalidadeResultado> GerarLoteAsync(GerarLoteMensalidadeRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Competencia))
        {
            throw new ArgumentException("A competência (MM/AAAA) é obrigatória.", nameof(request.Competencia));
        }

        var clubeId = _tenantContext.CurrentClubeId ?? request.ClubeId;
        var clube = await _db.Clubes.FirstOrDefaultAsync(c => c.Id == clubeId, ct)
            ?? throw new InvalidOperationException("Clube não encontrado.");

        var valor = request.ValorPersonalizado ?? clube.ValorMensalidadePadrao;
        if (valor <= 0)
        {
            throw new InvalidOperationException("O valor da mensalidade deve ser maior que zero.");
        }

        var competencia = request.Competencia.Trim();
        var associadosDoClube = await _db.Associados
            .Where(a => a.ClubeId == clubeId)
            .ToListAsync(ct);

        var existentes = await _db.Mensalidades
            .Where(m => m.ClubeId == clubeId && m.Competencia == competencia)
            .Select(m => m.AssociadoId)
            .ToListAsync(ct);

        var setExistentes = existentes.ToHashSet();

        var geradas = new List<Mensalidade>();
        var jaExistentes = 0;
        var isentos = 0;

        foreach (var associado in associadosDoClube)
        {
            if (!associado.DevePagarMensalidade())
            {
                isentos++;
                continue;
            }

            if (setExistentes.Contains(associado.Id))
            {
                jaExistentes++;
                continue;
            }

            var txId = $"CLB{competencia.Replace("/", "")}{associado.Id.ToString("N")[..6].ToUpperInvariant()}";
            var pix = PixPayloadGenerator.GerarPayload(
                clube.ChavePix,
                clube.Nome,
                clube.Cidade,
                valor,
                txId);

            var mensalidade = new Mensalidade
            {
                Id = Guid.NewGuid(),
                ClubeId = clubeId,
                AssociadoId = associado.Id,
                Associado = associado,
                Competencia = competencia,
                Valor = valor,
                DataVencimento = request.DataVencimento,
                Status = StatusMensalidade.Pendente,
                CodigoPixCopiaECola = pix,
                Observacoes = $"Lote {competencia}"
            };

            _db.Mensalidades.Add(mensalidade);
            geradas.Add(mensalidade);
        }

        if (geradas.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }

        return new GerarLoteMensalidadeResultado(
            geradas.Count,
            jaExistentes,
            isentos,
            geradas.Select(m => m.ToDto()).ToList());
    }

    public async Task<MensalidadeDto?> BaixarAsync(Guid mensalidadeId, string? observacoes = null, CancellationToken ct = default)
    {
        var mensalidade = await _db.Mensalidades
            .Include(m => m.Associado)
            .FirstOrDefaultAsync(m => m.Id == mensalidadeId, ct);

        if (mensalidade is null)
        {
            return null;
        }

        mensalidade.Status = StatusMensalidade.Paga;
        mensalidade.DataPagamento = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(observacoes))
        {
            mensalidade.Observacoes = observacoes.Trim();
        }

        await _db.SaveChangesAsync(ct);
        return mensalidade.ToDto();
    }
}
