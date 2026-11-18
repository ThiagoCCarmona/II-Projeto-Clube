using SQLite;

namespace Clube.Mobile.Models;

[Table("Eventos")]
public class LocalEventoItem
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string ClubeId { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string TipoEvento { get; set; } = string.Empty;
    public string PublicoAlvo { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string Local { get; set; } = string.Empty;
    public decimal ValorIngresso { get; set; }
    public int TotalConfirmados { get; set; }
    public bool UsuarioInscrito { get; set; }
}

[Table("Espacos")]
public class LocalEspacoItem
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string ClubeId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int Capacidade { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal TaxaReserva { get; set; }
}

[Table("Reservas")]
public class LocalReservaItem
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string EspacoId { get; set; } = string.Empty;
    public string EspacoNome { get; set; } = string.Empty;
    public string AssociadoNome { get; set; } = string.Empty;
    public string FinalidadeUso { get; set; } = string.Empty;
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;
    public bool CriadoOffline { get; set; }
    public bool PendenteSincronizacao { get; set; }
}

[Table("Mensalidades")]
public class LocalMensalidadeItem
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string Competencia { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CodigoPixCopiaECola { get; set; } = string.Empty;
    public string Observacoes { get; set; } = string.Empty;
}

[Table("Solicitacoes")]
public class LocalSolicitacaoItem
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? RespostaDiretoria { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataCriacao { get; set; }
    public bool PendenteSincronizacao { get; set; }
}

[Table("Dependentes")]
public class LocalDependenteItem
{
    [PrimaryKey]
    public string Id { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string GrauParentesco { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

[Table("Outbox")]
public class OutboxMutationItem
{
    [PrimaryKey]
    public string OutboxId { get; set; } = Guid.NewGuid().ToString();
    public string TipoOperacao { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
    public bool Sincronizado { get; set; }
    public string? UltimoErro { get; set; }
}
