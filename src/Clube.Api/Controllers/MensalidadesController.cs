using Clube.Application.Interfaces;
using Clube.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clube.Api.Controllers;

[ApiController]
[Route("api/mensalidades")]
[Authorize]
public class MensalidadesController : ControllerBase
{
    private readonly IMensalidadeService _mensalidadeService;

    public MensalidadesController(IMensalidadeService mensalidadeService)
    {
        _mensalidadeService = mensalidadeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MensalidadeDto>>> Listar(
        [FromQuery] Guid? clubeId,
        [FromQuery] Guid? associadoId,
        CancellationToken ct)
        => Ok(await _mensalidadeService.ListarAsync(clubeId, associadoId, ct));

    [HttpPost("gerar-lote")]
    [Authorize(Roles = "SuperAdmin,Presidente,Tesoureiro")]
    public async Task<ActionResult<GerarLoteMensalidadeResultado>> GerarLote([FromBody] GerarLoteMensalidadeRequest request, CancellationToken ct)
    {
        try
        {
            var resultado = await _mensalidadeService.GerarLoteAsync(request, ct);
            return Ok(resultado);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/baixar")]
    [Authorize(Roles = "SuperAdmin,Presidente,Tesoureiro")]
    public async Task<ActionResult<MensalidadeDto>> Baixar(Guid id, [FromBody] BaixarMensalidadeRequest? request, CancellationToken ct)
    {
        var baixada = await _mensalidadeService.BaixarAsync(id, request?.Observacoes, ct);
        return baixada is null ? NotFound() : Ok(baixada);
    }
}
