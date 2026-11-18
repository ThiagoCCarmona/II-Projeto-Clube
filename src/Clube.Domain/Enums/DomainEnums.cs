namespace Clube.Domain.Enums;

/// <summary>
/// Perfis de acesso disponíveis no painel Web e no aplicativo móvel.
/// </summary>
public enum PerfilUsuario
{
    SuperAdmin = 1,
    Presidente = 2,
    Tesoureiro = 3,
    Associado = 4,
    Dependente = 5,
    Simpatizante = 6
}

/// <summary>
/// Classificação estatutária do associado no clube.
/// </summary>
public enum TipoAssociado
{
    SocioFundadorComVoto = 1,
    SocioConvidadoSemVoto = 2,
    Simpatizante = 3
}

/// <summary>
/// Status de aprovação cadastral analisado pelo Presidente do clube.
/// </summary>
public enum StatusAprovacao
{
    Pendente = 1,
    Aprovado = 2,
    Rejeitado = 3
}

/// <summary>
/// Grau de parentesco de um dependente vinculado ao sócio titular.
/// </summary>
public enum GrauParentesco
{
    Conjuge = 1,
    Filho = 2,
    Outro = 3
}

/// <summary>
/// Categorias de eventos sociais e beneficentes do clube.
/// </summary>
public enum TipoEvento
{
    AlmocoDiaDasMaes = 1,
    JantarDiaDosPais = 2,
    CafeDaTarde = 3,
    FeijoadaBeneficente = 4,
    Geral = 5
}

/// <summary>
/// Restrição de público-alvo para participação e RSVP em eventos.
/// </summary>
public enum PublicoAlvoEvento
{
    SomenteAssociados = 1,
    AbertoAoPublico = 2,
    Simpatizantes = 3
}

/// <summary>
/// Situação da confirmação de presença (RSVP) em um evento.
/// </summary>
public enum StatusInscricao
{
    Confirmada = 1,
    Cancelada = 2
}

/// <summary>
/// Finalidade informada pelo associado ao solicitar reserva de espaço.
/// </summary>
public enum FinalidadeUsoEspaco
{
    Festa = 1,
    Churrasco = 2,
    Feijoada = 3,
    Outro = 4
}

/// <summary>
/// Ciclo de vida de uma reserva de espaço com detecção e mediação de conflitos.
/// </summary>
public enum StatusReserva
{
    PendenteAprovacao = 1,
    Confirmada = 2,
    Cancelada = 3,
    Rejeitada = 4,
    Conflito = 5,
    RejeitadaPorConflito = 6
}

/// <summary>
/// Situação financeira da mensalidade do associado.
/// </summary>
public enum StatusMensalidade
{
    Pendente = 1,
    Paga = 2,
    Atrasada = 3,
    Isenta = 4
}

/// <summary>
/// Tipo de registro aberto no SAC do associado.
/// </summary>
public enum TipoSolicitacao
{
    Solicitacao = 1,
    Reclamacao = 2
}

/// <summary>
/// Status do atendimento pela diretoria do clube.
/// </summary>
public enum StatusSolicitacao
{
    Aberta = 1,
    EmAnalise = 2,
    Respondida = 3
}

/// <summary>
/// Tipo da chave PIX cadastrada pelo clube para geração de cobranças EMV.
/// </summary>
public enum TipoChavePix
{
    CNPJ = 1,
    CPF = 2,
    Email = 3,
    Telefone = 4,
    Aleatoria = 5
}
