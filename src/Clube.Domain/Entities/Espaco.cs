using Clube.Domain.Enums;

namespace Clube.Domain.Entities;

/// <summary>
/// Espaço físico disponível para reserva pelos associados (ex.: Salão Social Principal).
/// </summary>
public class Espaco : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClubeId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Capacidade { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal TaxaReserva { get; set; }
    public bool Ativo { get; set; } = true;
}

/// <summary>
/// Reserva contínua de um espaço com início/fim que pode cruzar múltiplos dias.
/// </summary>
public class ReservaEspaco : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EspacoId { get; set; }
    public Espaco? Espaco { get; set; }
    public Guid ClubeId { get; set; }
    public Guid AssociadoId { get; set; }
    public Associado? Associado { get; set; }
    public FinalidadeUsoEspaco FinalidadeUso { get; set; } = FinalidadeUsoEspaco.Festa;
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public StatusReserva Status { get; set; } = StatusReserva.PendenteAprovacao;
    public string Observacoes { get; set; } = string.Empty;
    public bool CriadoOffline { get; set; }
    public DateTime? SincronizadoEm { get; set; }

    /// <summary>
    /// Valida se a reserva possui intervalo contínuo estritamente positivo.
    /// </summary>
    public bool PeriodoEhValido() => DataHoraFim > DataHoraInicio;

    /// <summary>
    /// Verifica sobreposição semi-aberta [inicio, fim): intervalos adjacentes não conflitam.
    /// </summary>
    public bool SobrepoeHorario(DateTime inicio, DateTime fim) =>
        DataHoraInicio < fim && DataHoraFim > inicio;

    /// <summary>
    /// Determina se esta reserva conflita no mesmo espaço com outra reserva distinta.
    /// </summary>
    public bool ConflitaCom(ReservaEspaco outra) =>
        Id != outra.Id &&
        EspacoId == outra.EspacoId &&
        SobrepoeHorario(outra.DataHoraInicio, outra.DataHoraFim);
}
