using Clube.Domain.Enums;

namespace Clube.Domain.Entities;

/// <summary>
/// Registro completo do associado ou simpatizante vinculado a um clube.
/// </summary>
public class Associado : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid ClubeId { get; set; }
    public Clube? Clube { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public TipoAssociado TipoAssociado { get; set; }
    public StatusAprovacao StatusAprovacao { get; set; } = StatusAprovacao.Pendente;
    public DateTime DataNascimento { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public string EstadoCivil { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string Interesses { get; set; } = string.Empty;
    public Guid? SocioTitularId { get; set; }
    public string? MotivoRejeicao { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public List<Dependente> Dependentes { get; set; } = [];

    /// <summary>
    /// Somente Sócios Fundadores com cadastro aprovado possuem direito a voto nas assembleias.
    /// </summary>
    public bool TemDireitoAVoto() =>
        TipoAssociado == TipoAssociado.SocioFundadorComVoto &&
        StatusAprovacao == StatusAprovacao.Aprovado;

    /// <summary>
    /// Apenas sócios titulares aprovados (Fundadores e Convidados) recebem mensalidade fixa em lote.
    /// Dependentes e Simpatizantes são isentos.
    /// </summary>
    public bool DevePagarMensalidade() =>
        StatusAprovacao == StatusAprovacao.Aprovado &&
        SocioTitularId == null &&
        (TipoAssociado == TipoAssociado.SocioFundadorComVoto ||
         TipoAssociado == TipoAssociado.SocioConvidadoSemVoto);

    /// <summary>
    /// Verifica se o associado atende ao público-alvo configurado para o evento.
    /// </summary>
    public bool PodeParticiparDoEvento(PublicoAlvoEvento publicoAlvo) =>
        publicoAlvo switch
        {
            PublicoAlvoEvento.AbertoAoPublico => true,
            PublicoAlvoEvento.Simpatizantes => StatusAprovacao == StatusAprovacao.Aprovado,
            PublicoAlvoEvento.SomenteAssociados =>
                StatusAprovacao == StatusAprovacao.Aprovado &&
                (TipoAssociado == TipoAssociado.SocioFundadorComVoto ||
                 TipoAssociado == TipoAssociado.SocioConvidadoSemVoto),
            _ => false
        };

    /// <summary>
    /// Aprova o cadastro do associado e limpa eventual motivo de rejeição anterior.
    /// </summary>
    public void Aprovar()
    {
        StatusAprovacao = StatusAprovacao.Aprovado;
        MotivoRejeicao = null;
    }

    /// <summary>
    /// Rejeita o cadastro exigindo justificativa obrigatória para transparência ao candidato.
    /// </summary>
    public void Rejeitar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ArgumentException("O motivo da rejeição é obrigatório.", nameof(motivo));
        }

        StatusAprovacao = StatusAprovacao.Rejeitado;
        MotivoRejeicao = motivo.Trim();
    }
}
