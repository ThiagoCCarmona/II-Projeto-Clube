namespace Clube.Domain.Entities;

/// <summary>
/// Contrato marcador para entidades isoladas por clube (multi-tenancy).
/// </summary>
public interface ITenantEntity
{
    /// <summary>
    /// Identificador do clube proprietário do registro.
    /// </summary>
    Guid ClubeId { get; set; }
}
