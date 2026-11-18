namespace Clube.Application.Interfaces;

/// <summary>
/// Contexto do tenant ativo na requisição ou sessão atual.
/// Quando <see cref="CurrentClubeId"/> é nulo e <see cref="IsSuperAdmin"/> é verdadeiro,
/// o sistema opera sem filtro de tenant para governança global da federação.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Identificador do clube atual ou nulo para visão global de SuperAdmin.
    /// </summary>
    Guid? CurrentClubeId { get; }

    /// <summary>
    /// Indica se o usuário atual possui privilégio global de SuperAdmin.
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// Identificador do usuário autenticado na operação.
    /// </summary>
    Guid? CurrentUsuarioId { get; }

    /// <summary>
    /// Identificador do associado vinculado ao usuário autenticado, quando aplicável.
    /// </summary>
    Guid? CurrentAssociadoId { get; }

    /// <summary>
    /// Define os dados de tenant e identidade para o escopo atual.
    /// </summary>
    void SetTenant(Guid? clubeId, bool isSuperAdmin = false, Guid? usuarioId = null, Guid? associadoId = null);
}
