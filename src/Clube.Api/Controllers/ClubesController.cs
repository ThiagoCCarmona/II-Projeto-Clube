using Clube.Application.Interfaces;
using Clube.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clube.Api.Controllers;

[ApiController]
[Route("api/clubes")]
public class ClubesController : ControllerBase
{
    private readonly IClubeService _clubeService;

    public ClubesController(IClubeService clubeService)
    {
        _clubeService = clubeService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ClubeDto>>> Listar(CancellationToken ct)
        => Ok(await _clubeService.ListarAsync(ct));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ClubeDto>> Obter(Guid id, CancellationToken ct)
    {
        var clube = await _clubeService.ObterPorIdAsync(id, ct);
        return clube is null ? NotFound() : Ok(clube);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ClubeDto>> Criar([FromBody] SalvarClubeRequest request, CancellationToken ct)
    {
        var clube = await _clubeService.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = clube.Id }, clube);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ClubeDto>> Atualizar(Guid id, [FromBody] SalvarClubeRequest request, CancellationToken ct)
    {
        var clube = await _clubeService.AtualizarAsync(id, request, ct);
        return clube is null ? NotFound() : Ok(clube);
    }
}
