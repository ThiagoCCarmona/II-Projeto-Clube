using Clube.Domain.Enums;

namespace Clube.Domain.Entities;

/// <summary>
/// Conta de autenticação do usuário no painel Web ou no aplicativo móvel.
/// </summary>
public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Nulo apenas para o perfil SuperAdmin que opera em todos os clubes.
    /// </summary>
    public Guid? ClubeId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
}
