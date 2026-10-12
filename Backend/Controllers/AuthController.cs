using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starlink.Api.Data;
using Starlink.Api.Dtos;
using Starlink.Api.Models;
using Starlink.Api.Services;

namespace Starlink.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(StarlinkDbContext db, TokenService tokenService) : ControllerBase
{
    private static readonly PasswordHasher<Dono> Hasher = new();

    [HttpPost("registrar")]
    public async Task<ActionResult<LoginResponseDto>> Registrar(RegistrarDonoDto dto)
    {
        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();

        if (await db.Donos.AnyAsync(d => d.Email == emailNormalizado))
            return Conflict("Já existe um dono cadastrado com esse email.");

        var dono = new Dono
        {
            Nome = dto.Nome.Trim(),
            Email = emailNormalizado,
            DataCadastro = DateTime.UtcNow
        };
        dono.SenhaHash = Hasher.HashPassword(dono, dto.Senha);

        db.Donos.Add(dono);
        await db.SaveChangesAsync();

        var (token, expiraEm) = tokenService.GerarToken(dono);
        return Ok(new LoginResponseDto
        {
            Token = token,
            ExpiraEm = expiraEm,
            DonoId = dono.Id,
            Nome = dono.Nome,
            Email = dono.Email
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
    {
        var emailNormalizado = dto.Email.Trim().ToLowerInvariant();
        var dono = await db.Donos.FirstOrDefaultAsync(d => d.Email == emailNormalizado);
        if (dono is null)
            return Unauthorized("Email ou senha inválidos.");

        var resultado = Hasher.VerifyHashedPassword(dono, dono.SenhaHash, dto.Senha);
        if (resultado == PasswordVerificationResult.Failed)
            return Unauthorized("Email ou senha inválidos.");

        var (token, expiraEm) = tokenService.GerarToken(dono);
        return Ok(new LoginResponseDto
        {
            Token = token,
            ExpiraEm = expiraEm,
            DonoId = dono.Id,
            Nome = dono.Nome,
            Email = dono.Email
        });
    }
}
