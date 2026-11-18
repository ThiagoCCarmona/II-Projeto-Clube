using Clube.Domain.Enums;

namespace Clube.Domain.Entities;

/// <summary>
/// Representa um clube afiliado à federação (tenant isolado do sistema).
/// </summary>
public class Clube
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nome { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ChavePix { get; set; } = string.Empty;
    public TipoChavePix TipoChavePix { get; set; } = TipoChavePix.CNPJ;
    public decimal ValorMensalidadePadrao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
