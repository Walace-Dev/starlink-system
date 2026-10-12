using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Starlink.Api.Config;
using Starlink.Api.Models;

namespace Starlink.Api.Services;

public class TokenService(IOptions<JwtSettings> jwtOptions)
{
    private readonly JwtSettings _settings = jwtOptions.Value;

    public (string Token, DateTime ExpiraEm) GerarToken(Dono dono)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_settings.ExpiraEmMinutos);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, dono.Id.ToString()),
            new Claim(ClaimTypes.Name, dono.Nome),
            new Claim(ClaimTypes.Email, dono.Email)
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
