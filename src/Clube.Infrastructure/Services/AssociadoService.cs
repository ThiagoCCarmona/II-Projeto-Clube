using Clube.Application.Interfaces;
using Clube.Domain.Entities;
using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Shared.DTOs;
using Clube.Shared.Validators;
using Microsoft.EntityFrameworkCore;

namespace Clube.Infrastructure.Services;

public class AssociadoService : IAssociadoService
{
    private readonly ClubeDbContext _db;
    private readonly ITenantContext _tenantContext;

    public AssociadoService(ClubeDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<AssociadoDto>> ListarAsync(Guid? clubeId = null, CancellationToken ct = default)
    {
        var query = _db.Associados
            .Include(a => a.Clube)
            .Include(a => a.Dependentes)
            .AsQueryable();

        if (clubeId.HasValue)
        {
            query = query.Where(a => a.ClubeId == clubeId.Value);
        }

        var itens = await query.OrderBy(a => a.Nome).ToListAsync(ct);
        return itens.Select(a => a.ToDto()).ToList();
    }

    public async Task<AssociadoDto?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var associado = await _db.Associados
            .Include(a => a.Clube)
            .Include(a => a.Dependentes)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        return associado?.ToDto();
    }

    public async Task<AssociadoDto> CriarAsync(CriarAssociadoRequest request, CancellationToken ct = default)
    {
        if (!CpfValidator.EhValido(request.CPF))
        {
            throw new ArgumentException("CPF inválido.", nameof(request.CPF));
        }

        var clubeAlvo = _tenantContext.CurrentClubeId ?? request.ClubeId;
        var cpfFormatado = CpfValidator.Formatar(request.CPF);

        if (await _db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.CPF == cpfFormatado, ct))
        {
            throw new InvalidOperationException("Já existe um usuário cadastrado com este CPF.");
        }

        var clube = await _db.Clubes.FirstOrDefaultAsync(c => c.Id == clubeAlvo, ct)
            ?? throw new InvalidOperationException("Clube não encontrado.");

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            ClubeId = clube.Id,
            Nome = request.Nome.Trim(),
            CPF = cpfFormatado,
            Email = request.Email.Trim(),
            Telefone = request.Telefone.Trim(),
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(string.IsNullOrWhiteSpace(request.Senha) ? "User@123" : request.Senha),
            Perfil = request.TipoAssociado == TipoAssociado.Simpatizante
                ? PerfilUsuario.Simpatizante
                : PerfilUsuario.Associado
        };

        var associado = new Associado
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            ClubeId = clube.Id,
            Nome = usuario.Nome,
            CPF = cpfFormatado,
            Email = usuario.Email,
            Telefone = usuario.Telefone,
            TipoAssociado = request.TipoAssociado,
            StatusAprovacao = request.AprovarAutomaticamente ? StatusAprovacao.Aprovado : StatusAprovacao.Pendente,
            DataNascimento = request.DataNascimento,
            Sexo = request.Sexo.Trim(),
            EstadoCivil = request.EstadoCivil.Trim(),
            Endereco = request.Endereco.Trim(),
            Bairro = request.Bairro.Trim(),
            Cidade = request.Cidade.Trim(),
            UF = request.UF.Trim().ToUpperInvariant(),
            Interesses = request.Interesses.Trim()
        };

        _db.Usuarios.Add(usuario);
        _db.Associados.Add(associado);
        await _db.SaveChangesAsync(ct);

        associado.Clube = clube;
        return associado.ToDto();
    }

    public async Task<AssociadoDto?> AtualizarAsync(Guid id, AtualizarAssociadoRequest request, CancellationToken ct = default)
    {
        var associado = await _db.Associados
            .Include(a => a.Clube)
            .Include(a => a.Dependentes)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (associado is null)
        {
            return null;
        }

        associado.Nome = request.Nome.Trim();
        associado.Email = request.Email.Trim();
        associado.Telefone = request.Telefone.Trim();
        associado.TipoAssociado = request.TipoAssociado;
        associado.Sexo = request.Sexo.Trim();
        associado.EstadoCivil = request.EstadoCivil.Trim();
        associado.Endereco = request.Endereco.Trim();
        associado.Bairro = request.Bairro.Trim();
        associado.Cidade = request.Cidade.Trim();
        associado.UF = request.UF.Trim().ToUpperInvariant();
        associado.Interesses = request.Interesses.Trim();

        await _db.SaveChangesAsync(ct);
        return associado.ToDto();
    }

    public async Task<AssociadoDto?> AprovarAsync(Guid id, CancellationToken ct = default)
    {
        var associado = await _db.Associados
            .Include(a => a.Clube)
            .Include(a => a.Dependentes)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (associado is null)
        {
            return null;
        }

        associado.Aprovar();
        await _db.SaveChangesAsync(ct);
        return associado.ToDto();
    }

    public async Task<AssociadoDto?> RejeitarAsync(Guid id, string motivo, CancellationToken ct = default)
    {
        var associado = await _db.Associados
            .Include(a => a.Clube)
            .Include(a => a.Dependentes)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (associado is null)
        {
            return null;
        }

        associado.Rejeitar(motivo);
        await _db.SaveChangesAsync(ct);
        return associado.ToDto();
    }

    public async Task<DependenteDto> AdicionarDependenteAsync(Guid associadoId, CriarDependenteRequest request, CancellationToken ct = default)
    {
        if (!CpfValidator.EhValido(request.CPF))
        {
            throw new ArgumentException("CPF do dependente inválido.", nameof(request.CPF));
        }

        var titular = await _db.Associados.FirstOrDefaultAsync(a => a.Id == associadoId, ct)
            ?? throw new InvalidOperationException("Associado titular não encontrado.");

        var cpfFormatado = CpfValidator.Formatar(request.CPF);

        // Cria também conta de acesso no aplicativo para o dependente caso o CPF ainda não exista
        Guid? usuarioId = null;
        var usuarioExistente = await _db.Usuarios.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.CPF == cpfFormatado, ct);
        if (usuarioExistente is null)
        {
            var usuarioDependente = new Usuario
            {
                Id = Guid.NewGuid(),
                ClubeId = titular.ClubeId,
                Nome = request.Nome.Trim(),
                CPF = cpfFormatado,
                Email = request.Email.Trim(),
                Telefone = request.Telefone.Trim(),
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(string.IsNullOrWhiteSpace(request.SenhaInicial) ? "User@123" : request.SenhaInicial),
                Perfil = PerfilUsuario.Dependente
            };
            _db.Usuarios.Add(usuarioDependente);
            usuarioId = usuarioDependente.Id;
        }
        else
        {
            usuarioId = usuarioExistente.Id;
        }

        var dependente = new Dependente
        {
            Id = Guid.NewGuid(),
            ClubeId = titular.ClubeId,
            AssociadoTitularId = titular.Id,
            UsuarioId = usuarioId,
            Nome = request.Nome.Trim(),
            CPF = cpfFormatado,
            GrauParentesco = request.GrauParentesco,
            DataNascimento = request.DataNascimento,
            Email = request.Email.Trim(),
            Telefone = request.Telefone.Trim(),
            Ativo = true
        };

        _db.Dependentes.Add(dependente);
        await _db.SaveChangesAsync(ct);
        return dependente.ToDto();
    }
}
