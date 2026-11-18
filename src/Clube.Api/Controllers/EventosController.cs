using Clube.Application.Interfaces;
using Clube.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clube.Api.Controllers;

[ApiController]
[Route("api/eventos")]
[Authorize]
public class EventosController : ControllerBase
{
    private readonly IEventoService _eventoService;

    public EventosController(IEventoService eventoService)
    {
        _eventoService = eventoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EventoDto>>> Listar([FromQuery] Guid? clubeId, CancellationToken ct)
        => Ok(await _eventoService.ListarAsync(clubeId, ct));

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Presidente")]
    public async Task<ActionResult<EventoDto>> Criar([FromBody] CriarEventoRequest request, CancellationToken ct)
    {
        try
        {
            var criado = await _eventoService.CriarAsync(request, ct);
            return Created($"/api/eventos/{criado.Id}", criado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/inscrever")]
    public async Task<ActionResult<InscricaoEventoDto>> Inscrever(Guid id, [FromBody] InscreverEventoRequest request, CancellationToken ct)
    {
        try
        {
            var inscricao = await _eventoService.InscreverAsync(id, request, ct);
            return Ok(inscricao);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/inscricoes")]
    public async Task<ActionResult<List<InscricaoEventoDto>>> ListarInscricoes(Guid id, CancellationToken ct)
        => Ok(await _eventoService.ListarInscricoesAsync(id, ct));
}
