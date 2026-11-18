using Clube.Application.Interfaces;
using Clube.Infrastructure.Persistence;
using Clube.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Clube.Infrastructure.Services;

public class ClubeService : IClubeService
{
    private readonly ClubeDbContext _db;

    public ClubeService(ClubeDbContext db)
    {
        _db = db;
    }

    public async Task<List<ClubeDto>> ListarAsync(CancellationToken ct = default)
    {
        var clubes = await _db.Clubes
            .OrderBy(c => c.Nome)
            .ToListAsync(ct);
        return clubes.Select(c => c.ToDto()).ToList();
    }

    public async Task<ClubeDto?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var clube = await _db.Clubes.FirstOrDefaultAsync(c => c.Id == id, ct);
        return clube?.ToDto();
    }

    public async Task<ClubeDto> CriarAsync(SalvarClubeRequest request, CancellationToken ct = default)
    {
        var clube = new Domain.Entities.Clube
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome.Trim(),
            Sigla = request.Sigla.Trim().ToUpperInvariant(),
            Endereco = request.Endereco.Trim(),
            Bairro = request.Bairro.Trim(),
            Cidade = request.Cidade.Trim(),
            UF = request.UF.Trim().ToUpperInvariant(),
            Telefone = request.Telefone.Trim(),
            Email = request.Email.Trim(),
            ChavePix = request.ChavePix.Trim(),
            TipoChavePix = request.TipoChavePix,
            ValorMensalidadePadrao = request.ValorMensalidadePadrao,
            Ativo = request.Ativo
        };

        _db.Clubes.Add(clube);
        await _db.SaveChangesAsync(ct);
        return clube.ToDto();
    }

    public async Task<ClubeDto?> AtualizarAsync(Guid id, SalvarClubeRequest request, CancellationToken ct = default)
    {
        var clube = await _db.Clubes.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (clube is null)
        {
            return null;
        }

        clube.Nome = request.Nome.Trim();
        clube.Sigla = request.Sigla.Trim().ToUpperInvariant();
        clube.Endereco = request.Endereco.Trim();
        clube.Bairro = request.Bairro.Trim();
        clube.Cidade = request.Cidade.Trim();
        clube.UF = request.UF.Trim().ToUpperInvariant();
        clube.Telefone = request.Telefone.Trim();
        clube.Email = request.Email.Trim();
        clube.ChavePix = request.ChavePix.Trim();
        clube.TipoChavePix = request.TipoChavePix;
        clube.ValorMensalidadePadrao = request.ValorMensalidadePadrao;
        clube.Ativo = request.Ativo;

        await _db.SaveChangesAsync(ct);
        return clube.ToDto();
    }
}
