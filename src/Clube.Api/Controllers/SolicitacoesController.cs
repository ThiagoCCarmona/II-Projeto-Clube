using Clube.Application.Interfaces;
using Clube.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clube.Api.Controllers;

[ApiController]
[Route("api/solicitacoes")]
[Authorize]
public class SolicitacoesController : ControllerBase
{
    private readonly ISolicitacaoService _solicitacaoService;

    public SolicitacoesController(ISolicitacaoService solicitacaoService)
    {
        _solicitacaoService = solicitacaoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SolicitacaoDto>>> Listar(
        [FromQuery] Guid? clubeId,
        [FromQuery] Guid? associadoId,
        CancellationToken ct)
        => Ok(await _solicitacaoService.ListarAsync(clubeId, associadoId, ct));

    [HttpPost]
    public async Task<ActionResult<SolicitacaoDto>> Criar([FromBody] CriarSolicitacaoRequest request, CancellationToken ct)
    {
        try
        {
            var criado = await _solicitacaoService.CriarAsync(request, ct);
            return Created($"/api/solicitacoes/{criado.Id}", criado);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/responder")]
    [Authorize(Roles = "SuperAdmin,Presidente,Tesoureiro")]
    public async Task<ActionResult<SolicitacaoDto>> Responder(Guid id, [FromBody] ResponderSolicitacaoRequest request, CancellationToken ct)
    {
        try
        {
            var respondido = await _solicitacaoService.ResponderAsync(id, request, ct);
            return respondido is null ? NotFound() : Ok(respondido);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
