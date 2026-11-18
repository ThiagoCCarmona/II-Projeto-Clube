using Clube.Domain.Enums;
using Clube.Shared.DTOs;

namespace Clube.Mobile.Services;

/// <summary>
/// Estado da sessão do associado autenticado no aplicativo móvel e controle de modo Online/Offline.
/// </summary>
public class SessionStateService
{
    public static readonly Guid DefaultClubeFozId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DefaultAssociadoFundadorId = Guid.Parse("a1000000-0000-0000-0000-000000000003");

    public Guid UsuarioId { get; private set; }
    public Guid ClubeId { get; private set; } = DefaultClubeFozId;
    public string ClubeNome { get; private set; } = "Rotary Foz Ponte";
    public Guid AssociadoId { get; private set; } = DefaultAssociadoFundadorId;
    public string Nome { get; private set; } = "Marcos Fundador Silva";
    public string CPF { get; private set; } = "333.333.333-33";
    public string Token { get; private set; } = string.Empty;
    public PerfilUsuario Perfil { get; private set; } = PerfilUsuario.Associado;
    public TipoAssociado TipoAssociado { get; private set; } = TipoAssociado.SocioFundadorComVoto;
    public StatusAprovacao StatusAprovacao { get; private set; } = StatusAprovacao.Aprovado;
    public bool TemDireitoAVoto { get; private set; } = true;
    public string Interesses { get; private set; } = "Pesca, Caca, Comida, Bebida";

    public bool ModoOfflineSimulado { get; set; }
    public DateTime? UltimaSincronizacaoUtc { get; set; }
    public int PendenciasOutbox { get; set; }

    public bool EstaConectado =>
        !ModoOfflineSimulado &&
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public string TextoStatusRede =>
        ModoOfflineSimulado
            ? "MODO OFFLINE (Simulado - Outbox Ativo)"
            : Connectivity.Current.NetworkAccess == NetworkAccess.Internet
                ? "ONLINE (API Local)"
                : "SEM REDE (Offline - Outbox Ativo)";

    public event Action? StateChanged;

    public void NotifyStateChanged() => StateChanged?.Invoke();

    public void AplicarLogin(LoginResponse login)
    {
        UsuarioId = login.UsuarioId;
        ClubeId = login.ClubeId ?? DefaultClubeFozId;
        ClubeNome = login.ClubeNome ?? "Rotary Foz Ponte";
        AssociadoId = login.AssociadoId ?? DefaultAssociadoFundadorId;
        Nome = login.Nome;
        CPF = login.CPF;
        Token = login.Token;
        Perfil = login.Perfil;
        TipoAssociado = login.TipoAssociado ?? TipoAssociado.SocioFundadorComVoto;
        StatusAprovacao = login.StatusAprovacao ?? StatusAprovacao.Aprovado;
        TemDireitoAVoto = login.TemDireitoAVoto;
        NotifyStateChanged();
    }

    public void EntrarModoOfflineDemonstracao(string cpf, string nome, TipoAssociado tipo, bool temVoto)
    {
        ClubeId = DefaultClubeFozId;
        ClubeNome = "Rotary Foz Ponte (Cache Local)";
        AssociadoId = DefaultAssociadoFundadorId;
        Nome = nome;
        CPF = cpf;
        Token = string.Empty;
        TipoAssociado = tipo;
        StatusAprovacao = StatusAprovacao.Aprovado;
        TemDireitoAVoto = temVoto;
        ModoOfflineSimulado = true;
        NotifyStateChanged();
    }

    public void AtualizarPerfilDoPull(AssociadoDto? associado, ClubeDto? clube)
    {
        if (clube is not null)
        {
            ClubeId = clube.Id;
            ClubeNome = clube.Nome;
        }

        if (associado is not null)
        {
            AssociadoId = associado.Id;
            Nome = associado.Nome;
            CPF = associado.CPF;
            TipoAssociado = associado.TipoAssociado;
            StatusAprovacao = associado.StatusAprovacao;
            TemDireitoAVoto = associado.TemDireitoAVoto;
            Interesses = associado.Interesses;
        }

        NotifyStateChanged();
    }
}
