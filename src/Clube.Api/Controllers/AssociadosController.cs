using Clube.Application.Interfaces;
using Clube.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clube.Api.Controllers;

[ApiController]
[Route("api/associados")]
[Authorize]
public class AssociadosController : ControllerBase
{
    private readonly IAssociadoService _associadoService;

    public AssociadosController(IAssociadoService associadoService)
    {
        _associadoService = associadoService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssociadoDto>>> Listar([FromQuery] Guid? clubeId, CancellationToken ct)
        => Ok(await _associadoService.ListarAsync(clubeId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssociadoDto>> Obter(Guid id, CancellationToken ct)
    {
        var associado = await _associadoService.ObterPorIdAsync(id, ct);
        return associado is null ? NotFound() : Ok(associado);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Presidente")]
    public async Task<ActionResult<AssociadoDto>> Criar([FromBody] CriarAssociadoRequest request, CancellationToken ct)
    {
        try
        {
            var criado = await _associadoService.CriarAsync(request, ct);
            return CreatedAtAction(nameof(Obter), new { id = criado.Id }, criado);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssociadoDto>> Atualizar(Guid id, [FromBody] AtualizarAssociadoRequest request, CancellationToken ct)
    {
        var atualizado = await _associadoService.AtualizarAsync(id, request, ct);
        return atualizado is null ? NotFound() : Ok(atualizado);
    }

    [HttpPost("{id:guid}/aprovar")]
    [Authorize(Roles = "SuperAdmin,Presidente")]
    public async Task<ActionResult<AssociadoDto>> Aprovar(Guid id, CancellationToken ct)
    {
        var aprovado = await _associadoService.AprovarAsync(id, ct);
        return aprovado is null ? NotFound() : Ok(aprovado);
    }

    [HttpPost("{id:guid}/rejeitar")]
    [Authorize(Roles = "SuperAdmin,Presidente")]
    public async Task<ActionResult<AssociadoDto>> Rejeitar(Guid id, [FromBody] RejeitarAssociadoRequest request, CancellationToken ct)
    {
        try
        {
            var rejeitado = await _associadoService.RejeitarAsync(id, request.Motivo, ct);
            return rejeitado is null ? NotFound() : Ok(rejeitado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/dependentes")]
    public async Task<ActionResult<DependenteDto>> AdicionarDependente(Guid id, [FromBody] CriarDependenteRequest request, CancellationToken ct)
    {
        try
        {
            var dependente = await _associadoService.AdicionarDependenteAsync(id, request, ct);
            return Created($"/api/associados/{id}", dependente);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
