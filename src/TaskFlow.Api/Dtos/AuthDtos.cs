using System.ComponentModel.DataAnnotations;

namespace TaskFlow.Api.Dtos;

public record RegisterRequest(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(int UserId, string Name, string Email, string Token, DateTime ExpiresAtUtc);
