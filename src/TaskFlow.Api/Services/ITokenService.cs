using TaskFlow.Domain.Entities;

namespace TaskFlow.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(AppUser user);
}
