using Clube.Domain.Enums;

namespace Clube.Domain.Entities;

/// <summary>
/// Cobrança mensal de associado titular com código PIX EMV Copia-e-Cola.
/// </summary>
public class Mensalidade : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClubeId { get; set; }
    public Guid AssociadoId { get; set; }
    public Associado? Associado { get; set; }
    public string Competencia { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public StatusMensalidade Status { get; set; } = StatusMensalidade.Pendente;
    public string CodigoPixCopiaECola { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;
}

/// <summary>
/// Registro de solicitação ou reclamação enviado pelo associado à diretoria do clube.
/// </summary>
public class SolicitacaoReclamacao : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClubeId { get; set; }
    public Guid AssociadoId { get; set; }
    public Associado? Associado { get; set; }
    public TipoSolicitacao Tipo { get; set; } = TipoSolicitacao.Solicitacao;
    public string Assunto { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? RespostaDiretoria { get; set; }
    public StatusSolicitacao Status { get; set; } = StatusSolicitacao.Aberta;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? DataResposta { get; set; }
}
