using Clube.Shared.DTOs;

namespace Clube.Application.Interfaces;

/// <summary>
/// Serviço de autenticação JWT e autocadastro de novos associados.
/// </summary>
public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AssociadoDto> RegistroPublicoAsync(RegistroPublicoRequest request, CancellationToken ct = default);
}

/// <summary>
/// Serviço de gestão dos clubes da federação (multi-tenant).
/// </summary>
public interface IClubeService
{
    Task<List<ClubeDto>> ListarAsync(CancellationToken ct = default);
    Task<ClubeDto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<ClubeDto> CriarAsync(SalvarClubeRequest request, CancellationToken ct = default);
    Task<ClubeDto?> AtualizarAsync(Guid id, SalvarClubeRequest request, CancellationToken ct = default);
}

/// <summary>
/// Serviço de gestão de associados, aprovação presidencial, direito a voto e dependentes.
/// </summary>
public interface IAssociadoService
{
    Task<List<AssociadoDto>> ListarAsync(Guid? clubeId = null, CancellationToken ct = default);
    Task<AssociadoDto?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<AssociadoDto> CriarAsync(CriarAssociadoRequest request, CancellationToken ct = default);
    Task<AssociadoDto?> AtualizarAsync(Guid id, AtualizarAssociadoRequest request, CancellationToken ct = default);
    Task<AssociadoDto?> AprovarAsync(Guid id, CancellationToken ct = default);
    Task<AssociadoDto?> RejeitarAsync(Guid id, string motivo, CancellationToken ct = default);
    Task<DependenteDto> AdicionarDependenteAsync(Guid associadoId, CriarDependenteRequest request, CancellationToken ct = default);
}

/// <summary>
/// Serviço de gestão de eventos sociais e confirmação de presença (RSVP).
/// </summary>
public interface IEventoService
{
    Task<List<EventoDto>> ListarAsync(Guid? clubeId = null, CancellationToken ct = default);
    Task<EventoDto> CriarAsync(CriarEventoRequest request, CancellationToken ct = default);
    Task<InscricaoEventoDto> InscreverAsync(Guid eventoId, InscreverEventoRequest request, CancellationToken ct = default);
    Task<List<InscricaoEventoDto>> ListarInscricoesAsync(Guid eventoId, CancellationToken ct = default);
}

/// <summary>
/// Serviço de gestão de espaços físicos e motor de reservas contínuas com mediação de conflitos.
/// </summary>
public interface IReservaService
{
    Task<List<EspacoDto>> ListarEspacosAsync(Guid? clubeId = null, CancellationToken ct = default);
    Task<EspacoDto> CriarEspacoAsync(CriarEspacoRequest request, CancellationToken ct = default);
    Task<List<ReservaEspacoDto>> ListarReservasAsync(Guid? clubeId = null, CancellationToken ct = default);
    Task<ReservaEspacoDto> SolicitarReservaAsync(CriarReservaRequest request, CancellationToken ct = default);
    Task<ReservaEspacoDto?> AprovarReservaAsync(Guid reservaId, CancellationToken ct = default);
    Task<ReservaEspacoDto?> RejeitarReservaAsync(Guid reservaId, string? motivo = null, CancellationToken ct = default);
}

/// <summary>
/// Serviço financeiro do tesoureiro para geração em lote com PIX EMV e baixa manual.
/// </summary>
public interface IMensalidadeService
{
    Task<List<MensalidadeDto>> ListarAsync(Guid? clubeId = null, Guid? associadoId = null, CancellationToken ct = default);
    Task<GerarLoteMensalidadeResultado> GerarLoteAsync(GerarLoteMensalidadeRequest request, CancellationToken ct = default);
    Task<MensalidadeDto?> BaixarAsync(Guid mensalidadeId, string? observacoes = null, CancellationToken ct = default);
}

/// <summary>
/// Serviço de solicitações e reclamações (SAC) entre associado e diretoria.
/// </summary>
public interface ISolicitacaoService
{
    Task<List<SolicitacaoDto>> ListarAsync(Guid? clubeId = null, Guid? associadoId = null, CancellationToken ct = default);
    Task<SolicitacaoDto> CriarAsync(CriarSolicitacaoRequest request, CancellationToken ct = default);
    Task<SolicitacaoDto?> ResponderAsync(Guid solicitacaoId, ResponderSolicitacaoRequest request, CancellationToken ct = default);
}

/// <summary>
/// Serviço de sincronização bidirecional (Push Outbox / Pull Snapshot) para o app móvel Offline-First.
/// </summary>
public interface ISyncService
{
    Task<SyncPushResponse> PushAsync(SyncPushRequest request, CancellationToken ct = default);
    Task<SyncPullResponse> PullAsync(Guid clubeId, Guid? associadoId, CancellationToken ct = default);
}
