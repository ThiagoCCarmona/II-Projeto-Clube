using Clube.Application.Interfaces;
using Clube.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clube.Api.Controllers;

[ApiController]
[Route("api/sync")]
[Authorize]
public class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly ITenantContext _tenantContext;

    public SyncController(ISyncService syncService, ITenantContext tenantContext)
    {
        _syncService = syncService;
        _tenantContext = tenantContext;
    }

    [HttpPost("push")]
    public async Task<ActionResult<SyncPushResponse>> Push([FromBody] SyncPushRequest request, CancellationToken ct)
        => Ok(await _syncService.PushAsync(request, ct));

    [HttpGet("pull")]
    public async Task<ActionResult<SyncPullResponse>> Pull(
        [FromQuery] Guid? clubeId,
        [FromQuery] Guid? associadoId,
        CancellationToken ct)
    {
        var clubeAlvo = _tenantContext.CurrentClubeId ?? clubeId ?? Guid.Empty;
        if (clubeAlvo == Guid.Empty)
        {
            return BadRequest(new { message = "clubeId é obrigatório para sincronização." });
        }

        var associadoAlvo = associadoId ?? _tenantContext.CurrentAssociadoId;
        return Ok(await _syncService.PullAsync(clubeAlvo, associadoAlvo, ct));
    }
}
