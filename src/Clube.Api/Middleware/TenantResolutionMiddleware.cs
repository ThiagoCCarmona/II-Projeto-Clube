using System.Security.Claims;
using Clube.Application.Interfaces;
using Clube.Domain.Enums;

namespace Clube.Api.Middleware;

/// <summary>
/// Popula o ITenantContext a partir das claims do JWT autenticado antes da execução dos controllers.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var role = context.User.FindFirstValue(ClaimTypes.Role);
            var isSuperAdmin = string.Equals(role, nameof(PerfilUsuario.SuperAdmin), StringComparison.OrdinalIgnoreCase);

            Guid? usuarioId = Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : null;
            Guid? associadoId = Guid.TryParse(context.User.FindFirstValue("associadoId"), out var aid) ? aid : null;
            Guid? clubeId = Guid.TryParse(context.User.FindFirstValue("clubeId"), out var cid) ? cid : null;

            if (isSuperAdmin)
            {
                clubeId = Guid.TryParse(context.Request.Headers["X-Clube-Id"].FirstOrDefault(), out var headerClubeId)
                    ? headerClubeId
                    : null;
            }

            tenantContext.SetTenant(clubeId, isSuperAdmin, usuarioId, associadoId);
        }
        else
        {
            tenantContext.SetTenant(null, isSuperAdmin: false);
        }

        await _next(context);
    }
}
