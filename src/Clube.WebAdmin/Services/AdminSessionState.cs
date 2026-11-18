using Clube.Application.Interfaces;
using Clube.Domain.Enums;
using Clube.Infrastructure.Persistence;
using Clube.Shared.DTOs;

namespace Clube.WebAdmin.Services;

/// <summary>
/// Estado de sessão do painel administrativo WebAdmin com seletor de perfis para demonstração acadêmica.
/// </summary>
public class AdminSessionState
{
    private readonly ITenantContext _tenantContext;
    private readonly ClubeDbContext _db;

    public AdminSessionState(ITenantContext tenantContext, ClubeDbContext db)
    {
        _tenantContext = tenantContext;
        _db = db;

        // Inicia no perfil Presidente Rotary Foz (Carlos Kaneko) pronto para governança imediata
        SelecionarPerfilDemo("presidente-foz");
    }

    public Guid? UsuarioId { get; private set; }
    public Guid? ClubeId { get; private set; }
    public string ClubeNome { get; private set; } = "Rotary Foz Ponte";
    public string NomeUsuario { get; private set; } = "Carlos Kaneko";
    public string CPF { get; private set; } = "111.111.111-11";
    public PerfilUsuario Perfil { get; private set; } = PerfilUsuario.Presidente;
    public string ChavePerfilAtual { get; private set; } = "presidente-foz";

    public bool EhSuperAdmin => Perfil == PerfilUsuario.SuperAdmin;
    public bool PodeGerenciarClubes => Perfil == PerfilUsuario.SuperAdmin;
    public bool PodeGerenciarAssociadosEReservas => Perfil is PerfilUsuario.SuperAdmin or PerfilUsuario.Presidente;
    public bool PodeGerenciarFinanceiro => Perfil is PerfilUsuario.SuperAdmin or PerfilUsuario.Presidente or PerfilUsuario.Tesoureiro;

    public event Action? OnSessionChanged;

    public void AplicarNoEscopo()
    {
        _tenantContext.SetTenant(ClubeId, EhSuperAdmin, UsuarioId, null);
    }

    public void SelecionarPerfilDemo(string chave)
    {
        ChavePerfilAtual = chave;
        switch (chave)
        {
            case "superadmin":
                UsuarioId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                ClubeId = null;
                ClubeNome = "Todos os Clubes (Federacao)";
                NomeUsuario = "Super Administrador";
                CPF = "000.000.000-00";
                Perfil = PerfilUsuario.SuperAdmin;
                break;

            case "tesoureiro-foz":
                UsuarioId = Guid.Parse("10000000-0000-0000-0000-000000000002");
                ClubeId = DbInitializer.ClubeFozId;
                ClubeNome = "Rotary Foz Ponte";
                NomeUsuario = "Helena Tesoureira";
                CPF = "222.222.222-22";
                Perfil = PerfilUsuario.Tesoureiro;
                break;

            case "presidente-guaira":
                UsuarioId = Guid.Parse("20000000-0000-0000-0000-000000000001");
                ClubeId = DbInitializer.ClubeGuairaId;
                ClubeNome = "Iate Clube Guaira";
                NomeUsuario = "Eduardo Presidente Guaira";
                CPF = "666.666.666-66";
                Perfil = PerfilUsuario.Presidente;
                break;

            default: // presidente-foz
                ChavePerfilAtual = "presidente-foz";
                UsuarioId = Guid.Parse("10000000-0000-0000-0000-000000000001");
                ClubeId = DbInitializer.ClubeFozId;
                ClubeNome = "Rotary Foz Ponte";
                NomeUsuario = "Carlos Kaneko";
                CPF = "111.111.111-11";
                Perfil = PerfilUsuario.Presidente;
                break;
        }

        _db.ChangeTracker.Clear();
        AplicarNoEscopo();
        OnSessionChanged?.Invoke();
    }

    public void AplicarLogin(LoginResponse login)
    {
        UsuarioId = login.UsuarioId;
        ClubeId = login.ClubeId;
        ClubeNome = login.ClubeNome ?? "Todos os Clubes (Federacao)";
        NomeUsuario = login.Nome;
        CPF = login.CPF;
        Perfil = login.Perfil;
        ChavePerfilAtual = "custom";

        _db.ChangeTracker.Clear();
        AplicarNoEscopo();
        OnSessionChanged?.Invoke();
    }
}
