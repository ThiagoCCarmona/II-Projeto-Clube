using Clube.Application.Interfaces;
using Clube.Domain.Entities;
using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Infrastructure.Security;
using Clube.Shared.DTOs;
using Clube.Shared.Validators;
using Microsoft.EntityFrameworkCore;

namespace Clube.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ClubeDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly JwtTokenService _tokenService;

    public AuthService(ClubeDbContext db, ITenantContext tenantContext, JwtTokenService tokenService)
    {
        _db = db;
        _tenantContext = tenantContext;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var cpfLimpo = CpfValidator.Limpar(request.Cpf);
        if (string.IsNullOrWhiteSpace(cpfLimpo))
        {
            return null;
        }

        var cpfFormatado = CpfValidator.Formatar(cpfLimpo);

        var usuario = await _db.Usuarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Ativo && (u.CPF == cpfFormatado || u.CPF == cpfLimpo), ct);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            return null;
        }

        var associado = await _db.Associados
            .IgnoreQueryFilters()
            .Include(a => a.Clube)
            .FirstOrDefaultAsync(a => a.UsuarioId == usuario.Id, ct);

        var clube = usuario.ClubeId.HasValue
            ? await _db.Clubes.FirstOrDefaultAsync(c => c.Id == usuario.ClubeId.Value, ct)
            : null;

        var token = _tokenService.GerarToken(usuario, associado?.Id);

        return new LoginResponse(
            token,
            usuario.Id,
            usuario.ClubeId,
            clube?.Nome,
            associado?.Id,
            usuario.Nome,
            usuario.CPF,
            usuario.Perfil,
            associado?.TipoAssociado,
            associado?.StatusAprovacao,
            associado?.TemDireitoAVoto() ?? false);
    }

    public async Task<AssociadoDto> RegistroPublicoAsync(RegistroPublicoRequest request, CancellationToken ct = default)
    {
        if (!CpfValidator.EhValido(request.CPF))
        {
            throw new ArgumentException("CPF inválido segundo a regra matemática de dígitos verificadores.", nameof(request.CPF));
        }

        var cpfFormatado = CpfValidator.Formatar(request.CPF);
        var jaExiste = await _db.Usuarios.IgnoreQueryFilters().AnyAsync(u => u.CPF == cpfFormatado, ct);
        if (jaExiste)
        {
            throw new InvalidOperationException("Já existe um usuário cadastrado com este CPF.");
        }

        var clube = await _db.Clubes.FirstOrDefaultAsync(c => c.Id == request.ClubeId && c.Ativo, ct)
            ?? throw new InvalidOperationException("Clube selecionado não encontrado ou inativo.");

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
            StatusAprovacao = StatusAprovacao.Pendente,
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
}
