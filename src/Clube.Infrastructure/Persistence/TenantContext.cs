using Clube.Application.Interfaces;

namespace Clube.Infrastructure.Persistence;

/// <summary>
/// Implementação scoped do contexto de tenant ativo para EF Core e serviços de negócio.
/// </summary>
public class TenantContext : ITenantContext
{
    public Guid? CurrentClubeId { get; private set; }
    public bool IsSuperAdmin { get; private set; }
    public Guid? CurrentUsuarioId { get; private set; }
    public Guid? CurrentAssociadoId { get; private set; }

    public void SetTenant(
        Guid? clubeId,
        bool isSuperAdmin = false,
        Guid? usuarioId = null,
        Guid? associadoId = null)
    {
        CurrentClubeId = clubeId;
        IsSuperAdmin = isSuperAdmin;
        CurrentUsuarioId = usuarioId;
        CurrentAssociadoId = associadoId;
    }
}
