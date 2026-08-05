using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Api.Dtos;
using TaskFlow.Api.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Data;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public AuthController(AppDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var emailNormalized = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == emailNormalized))
        {
            return Conflict(new { message = "Já existe um usuário cadastrado com este e-mail." });
        }

        var user = new AppUser
        {
            Name = request.Name.Trim(),
            Email = emailNormalized
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var (token, expiresAtUtc) = _tokenService.GenerateToken(user);
        return CreatedAtAction(nameof(Register), new AuthResponse(user.Id, user.Name, user.Email, token, expiresAtUtc));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var emailNormalized = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == emailNormalized);

        if (user is null)
        {
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "E-mail ou senha inválidos." });
        }

        var (token, expiresAtUtc) = _tokenService.GenerateToken(user);
        return Ok(new AuthResponse(user.Id, user.Name, user.Email, token, expiresAtUtc));
    }
}
