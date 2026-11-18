using Clube.Application.Interfaces;
using Clube.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clube.Api.Controllers;

[ApiController]
[Route("api/reservas")]
[Authorize]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public ReservasController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReservaEspacoDto>>> Listar([FromQuery] Guid? clubeId, CancellationToken ct)
        => Ok(await _reservaService.ListarReservasAsync(clubeId, ct));

    [HttpPost]
    public async Task<ActionResult<ReservaEspacoDto>> Solicitar([FromBody] CriarReservaRequest request, CancellationToken ct)
    {
        try
        {
            var reserva = await _reservaService.SolicitarReservaAsync(request, ct);
            return Created($"/api/reservas/{reserva.Id}", reserva);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Roles = "SuperAdmin,Presidente")]
    public async Task<ActionResult<ReservaEspacoDto>> Aprovar(Guid id, CancellationToken ct)
    {
        try
        {
            var aprovada = await _reservaService.AprovarReservaAsync(id, ct);
            return aprovada is null ? NotFound() : Ok(aprovada);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/rejeitar")]
    [Authorize(Roles = "SuperAdmin,Presidente")]
    public async Task<ActionResult<ReservaEspacoDto>> Rejeitar(Guid id, [FromBody] RejeitarReservaRequest? request, CancellationToken ct)
    {
        var rejeitada = await _reservaService.RejeitarReservaAsync(id, request?.Motivo, ct);
        return rejeitada is null ? NotFound() : Ok(rejeitada);
    }
}
