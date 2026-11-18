using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clube.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Clube.Infrastructure.Security;

/// <summary>
/// Gerador de tokens JWT contendo claims de usuário, perfil e tenant (clubeId).
/// </summary>
public class JwtTokenService
{
    public const string DefaultDevKey = "ProjetoClube_SuperSecretAcademicKey_2026_Min32Bytes!";
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"] ?? DefaultDevKey;
        _issuer = configuration["Jwt:Issuer"] ?? "Clube.Api";
        _audience = configuration["Jwt:Audience"] ?? "Clube.Clients";
    }

    public string GerarToken(Usuario usuario, Guid? associadoId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Role, usuario.Perfil.ToString()),
            new("cpf", usuario.CPF)
        };

        if (usuario.ClubeId.HasValue)
        {
            claims.Add(new Claim("clubeId", usuario.ClubeId.Value.ToString()));
        }

        if (associadoId.HasValue)
        {
            claims.Add(new Claim("associadoId", associadoId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(12),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
