using Clube.Application.Interfaces;
using Clube.Domain.Entities;
using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Clube.Infrastructure.Services;

public class SolicitacaoService : ISolicitacaoService
{
    private readonly ClubeDbContext _db;
    private readonly ITenantContext _tenantContext;

    public SolicitacaoService(ClubeDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<SolicitacaoDto>> ListarAsync(Guid? clubeId = null, Guid? associadoId = null, CancellationToken ct = default)
    {
        var query = _db.SolicitacoesReclamacoes
            .Include(s => s.Associado)
            .AsQueryable();

        if (clubeId.HasValue)
        {
            query = query.Where(s => s.ClubeId == clubeId.Value);
        }

        if (associadoId.HasValue)
        {
            query = query.Where(s => s.AssociadoId == associadoId.Value);
        }

        var itens = await query.OrderByDescending(s => s.DataCriacao).ToListAsync(ct);
        return itens.Select(s => s.ToDto()).ToList();
    }

    public async Task<SolicitacaoDto> CriarAsync(CriarSolicitacaoRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Assunto) || string.IsNullOrWhiteSpace(request.Descricao))
        {
            throw new ArgumentException("Assunto e descrição são obrigatórios.");
        }

        if (request.Id.HasValue)
        {
            var existente = await _db.SolicitacoesReclamacoes
                .Include(s => s.Associado)
                .FirstOrDefaultAsync(s => s.Id == request.Id.Value, ct);
            if (existente is not null)
            {
                return existente.ToDto();
            }
        }

        var associado = await _db.Associados.FirstOrDefaultAsync(a => a.Id == request.AssociadoId, ct)
            ?? throw new InvalidOperationException("Associado não encontrado.");

        var clubeId = _tenantContext.CurrentClubeId ?? associado.ClubeId;

        var registro = new SolicitacaoReclamacao
        {
            Id = request.Id ?? Guid.NewGuid(),
            ClubeId = clubeId,
            AssociadoId = associado.Id,
            Associado = associado,
            Tipo = request.Tipo,
            Assunto = request.Assunto.Trim(),
            Descricao = request.Descricao.Trim(),
            Status = StatusSolicitacao.Aberta,
            DataCriacao = DateTime.UtcNow
        };

        _db.SolicitacoesReclamacoes.Add(registro);
        await _db.SaveChangesAsync(ct);
        return registro.ToDto();
    }

    public async Task<SolicitacaoDto?> ResponderAsync(Guid solicitacaoId, ResponderSolicitacaoRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Resposta))
        {
            throw new ArgumentException("A resposta da diretoria é obrigatória.", nameof(request));
        }

        var registro = await _db.SolicitacoesReclamacoes
            .Include(s => s.Associado)
            .FirstOrDefaultAsync(s => s.Id == solicitacaoId, ct);

        if (registro is null)
        {
            return null;
        }

        registro.RespostaDiretoria = request.Resposta.Trim();
        registro.Status = request.Status ?? StatusSolicitacao.Respondida;
        registro.DataResposta = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
        return registro.ToDto();
    }
}
