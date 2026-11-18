using Clube.Domain.Enums;

namespace Clube.Domain.Entities;

/// <summary>
/// Dependente vinculado a um associado titular com acesso ao app e eventos.
/// </summary>
public class Dependente : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClubeId { get; set; }
    public Guid AssociadoTitularId { get; set; }
    public Associado? AssociadoTitular { get; set; }
    public Guid? UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public GrauParentesco GrauParentesco { get; set; }
    public DateTime DataNascimento { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}
