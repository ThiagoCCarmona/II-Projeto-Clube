using Clube.Application.Interfaces;
using Clube.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clube.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var response = await _authService.LoginAsync(request, ct);
        if (response is null)
        {
            return Unauthorized(new { message = "CPF ou senha inválidos." });
        }

        return Ok(response);
    }

    [HttpPost("registro-publico")]
    [AllowAnonymous]
    public async Task<ActionResult<AssociadoDto>> RegistroPublico([FromBody] RegistroPublicoRequest request, CancellationToken ct)
    {
        try
        {
            var criado = await _authService.RegistroPublicoAsync(request, ct);
            return Created($"/api/associados/{criado.Id}", criado);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
