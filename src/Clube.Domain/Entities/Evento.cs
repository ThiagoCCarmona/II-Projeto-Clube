using Clube.Domain.Enums;

namespace Clube.Domain.Entities;

/// <summary>
/// Evento social ou beneficente promovido pelo clube.
/// </summary>
public class Evento : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClubeId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public TipoEvento TipoEvento { get; set; } = TipoEvento.Geral;
    public PublicoAlvoEvento PublicoAlvo { get; set; } = PublicoAlvoEvento.AbertoAoPublico;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Local { get; set; } = string.Empty;
    public decimal ValorIngresso { get; set; }
    public bool RequerInscricao { get; set; } = true;
    public bool Ativo { get; set; } = true;

    public List<InscricaoEvento> Inscricoes { get; set; } = [];
}

/// <summary>
/// Confirmação de presença (RSVP) de um associado em um evento do clube.
/// </summary>
public class InscricaoEvento : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClubeId { get; set; }
    public Guid EventoId { get; set; }
    public Evento? Evento { get; set; }
    public Guid AssociadoId { get; set; }
    public Associado? Associado { get; set; }
    public DateTime DataInscricao { get; set; } = DateTime.UtcNow;
    public int QuantidadeAcompanhantes { get; set; }
    public StatusInscricao Status { get; set; } = StatusInscricao.Confirmada;
}
