using Clube.Application.Interfaces;
using Clube.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clube.Api.Controllers;

[ApiController]
[Route("api/espacos")]
[Authorize]
public class EspacosController : ControllerBase
{
    private readonly IReservaService _reservaService;

    public EspacosController(IReservaService reservaService)
    {
        _reservaService = reservaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EspacoDto>>> Listar([FromQuery] Guid? clubeId, CancellationToken ct)
        => Ok(await _reservaService.ListarEspacosAsync(clubeId, ct));

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Presidente")]
    public async Task<ActionResult<EspacoDto>> Criar([FromBody] CriarEspacoRequest request, CancellationToken ct)
    {
        var criado = await _reservaService.CriarEspacoAsync(request, ct);
        return Created($"/api/espacos/{criado.Id}", criado);
    }
}
