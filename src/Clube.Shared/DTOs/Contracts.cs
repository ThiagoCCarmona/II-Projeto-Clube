using Clube.Domain.Enums;

namespace Clube.Shared.DTOs;

// ==================== AUTH ====================
public record LoginRequest(string Cpf, string Senha);

public record LoginResponse(
    string Token,
    Guid UsuarioId,
    Guid? ClubeId,
    string? ClubeNome,
    Guid? AssociadoId,
    string Nome,
    string CPF,
    PerfilUsuario Perfil,
    TipoAssociado? TipoAssociado,
    StatusAprovacao? StatusAprovacao,
    bool TemDireitoAVoto);

public record RegistroPublicoRequest(
    Guid ClubeId,
    string Nome,
    string CPF,
    string Email,
    string Telefone,
    string Senha,
    TipoAssociado TipoAssociado,
    DateTime DataNascimento,
    string Sexo,
    string EstadoCivil,
    string Endereco,
    string Bairro,
    string Cidade,
    string UF,
    string Interesses);

// ==================== CLUBE ====================
public record ClubeDto(
    Guid Id,
    string Nome,
    string Sigla,
    string Endereco,
    string Bairro,
    string Cidade,
    string UF,
    string Telefone,
    string Email,
    string ChavePix,
    TipoChavePix TipoChavePix,
    decimal ValorMensalidadePadrao,
    bool Ativo,
    DateTime DataCadastro);

public record SalvarClubeRequest(
    string Nome,
    string Sigla,
    string Endereco,
    string Bairro,
    string Cidade,
    string UF,
    string Telefone,
    string Email,
    string ChavePix,
    TipoChavePix TipoChavePix,
    decimal ValorMensalidadePadrao,
    bool Ativo = true);

// ==================== ASSOCIADO E DEPENDENTES ====================
public record DependenteDto(
    Guid Id,
    Guid ClubeId,
    Guid AssociadoTitularId,
    string Nome,
    string CPF,
    GrauParentesco GrauParentesco,
    DateTime DataNascimento,
    string Email,
    string Telefone,
    bool Ativo);

public record CriarDependenteRequest(
    string Nome,
    string CPF,
    GrauParentesco GrauParentesco,
    DateTime DataNascimento,
    string Email,
    string Telefone,
    string? SenhaInicial = null);

public record AssociadoDto(
    Guid Id,
    Guid UsuarioId,
    Guid ClubeId,
    string ClubeNome,
    string Nome,
    string CPF,
    string Email,
    string Telefone,
    TipoAssociado TipoAssociado,
    StatusAprovacao StatusAprovacao,
    bool TemDireitoAVoto,
    DateTime DataNascimento,
    string Sexo,
    string EstadoCivil,
    string Endereco,
    string Bairro,
    string Cidade,
    string UF,
    string Interesses,
    Guid? SocioTitularId,
    string? MotivoRejeicao,
    DateTime DataCadastro,
    List<DependenteDto> Dependentes);

public record CriarAssociadoRequest(
    Guid ClubeId,
    string Nome,
    string CPF,
    string Email,
    string Telefone,
    string Senha,
    TipoAssociado TipoAssociado,
    DateTime DataNascimento,
    string Sexo,
    string EstadoCivil,
    string Endereco,
    string Bairro,
    string Cidade,
    string UF,
    string Interesses,
    bool AprovarAutomaticamente = false);

public record AtualizarAssociadoRequest(
    string Nome,
    string Email,
    string Telefone,
    TipoAssociado TipoAssociado,
    string Sexo,
    string EstadoCivil,
    string Endereco,
    string Bairro,
    string Cidade,
    string UF,
    string Interesses);

public record RejeitarAssociadoRequest(string Motivo);

// ==================== EVENTOS ====================
public record EventoDto(
    Guid Id,
    Guid ClubeId,
    string Titulo,
    string Descricao,
    TipoEvento TipoEvento,
    PublicoAlvoEvento PublicoAlvo,
    DateTime DataInicio,
    DateTime DataFim,
    string Local,
    decimal ValorIngresso,
    bool RequerInscricao,
    bool Ativo,
    int TotalConfirmados);

public record CriarEventoRequest(
    Guid ClubeId,
    string Titulo,
    string Descricao,
    TipoEvento TipoEvento,
    PublicoAlvoEvento PublicoAlvo,
    DateTime DataInicio,
    DateTime DataFim,
    string Local,
    decimal ValorIngresso,
    bool RequerInscricao = true);

public record InscreverEventoRequest(Guid AssociadoId, int QuantidadeAcompanhantes);

public record InscricaoEventoDto(
    Guid Id,
    Guid EventoId,
    string EventoTitulo,
    Guid AssociadoId,
    string AssociadoNome,
    DateTime DataInscricao,
    int QuantidadeAcompanhantes,
    StatusInscricao Status);

// ==================== ESPAÇOS E RESERVAS ====================
public record EspacoDto(
    Guid Id,
    Guid ClubeId,
    string Nome,
    int Capacidade,
    string Descricao,
    decimal TaxaReserva,
    bool Ativo);

public record CriarEspacoRequest(
    Guid ClubeId,
    string Nome,
    int Capacidade,
    string Descricao,
    decimal TaxaReserva);

public record ReservaEspacoDto(
    Guid Id,
    Guid EspacoId,
    string EspacoNome,
    Guid ClubeId,
    Guid AssociadoId,
    string AssociadoNome,
    FinalidadeUsoEspaco FinalidadeUso,
    DateTime DataHoraInicio,
    DateTime DataHoraFim,
    StatusReserva Status,
    string Observacoes,
    bool CriadoOffline,
    DateTime? SincronizadoEm,
    bool TemConflitoPendente);

public record CriarReservaRequest(
    Guid? Id,
    Guid EspacoId,
    Guid ClubeId,
    Guid AssociadoId,
    FinalidadeUsoEspaco FinalidadeUso,
    DateTime DataHoraInicio,
    DateTime DataHoraFim,
    string Observacoes,
    bool CriadoOffline = false);

public record RejeitarReservaRequest(string? Motivo);

// ==================== MENSALIDADES / PIX ====================
public record MensalidadeDto(
    Guid Id,
    Guid ClubeId,
    Guid AssociadoId,
    string AssociadoNome,
    string AssociadoCpf,
    TipoAssociado TipoAssociado,
    string Competencia,
    decimal Valor,
    DateTime DataVencimento,
    DateTime? DataPagamento,
    StatusMensalidade Status,
    string CodigoPixCopiaECola,
    string Observacoes);

public record GerarLoteMensalidadeRequest(
    Guid ClubeId,
    string Competencia,
    DateTime DataVencimento,
    decimal? ValorPersonalizado = null);

public record GerarLoteMensalidadeResultado(
    int Geradas,
    int JaExistentes,
    int IsentosOuNaoTitulares,
    List<MensalidadeDto> MensalidadesGeradas);

public record BaixarMensalidadeRequest(string? Observacoes);

// ==================== SOLICITAÇÕES / SAC ====================
public record SolicitacaoDto(
    Guid Id,
    Guid ClubeId,
    Guid AssociadoId,
    string AssociadoNome,
    TipoSolicitacao Tipo,
    string Assunto,
    string Descricao,
    string? RespostaDiretoria,
    StatusSolicitacao Status,
    DateTime DataCriacao,
    DateTime? DataResposta);

public record CriarSolicitacaoRequest(
    Guid? Id,
    Guid ClubeId,
    Guid AssociadoId,
    TipoSolicitacao Tipo,
    string Assunto,
    string Descricao);

public record ResponderSolicitacaoRequest(string Resposta, StatusSolicitacao? Status = null);

// ==================== SINCRONIZAÇÃO OFFLINE-FIRST ====================
public record SyncMutationDto(
    Guid OutboxId,
    string TipoOperacao,
    string PayloadJson,
    DateTime CriadoEmUtc);

public record SyncPushRequest(Guid ClubeId, Guid? AssociadoId, List<SyncMutationDto> Mutacoes);

public record SyncItemResultDto(
    Guid OutboxId,
    bool Sucesso,
    string StatusResultante,
    string Mensagem,
    Guid? EntidadeId);

public record SyncPushResponse(List<SyncItemResultDto> Resultados, DateTime SincronizadoEmUtc);

public record SyncPullResponse(
    ClubeDto? Clube,
    AssociadoDto? Associado,
    List<EspacoDto> Espacos,
    List<EventoDto> Eventos,
    List<ReservaEspacoDto> Reservas,
    List<MensalidadeDto> Mensalidades,
    List<SolicitacaoDto> Solicitacoes,
    DateTime TimestampUtc);
