using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskFlow.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (value is null || !int.TryParse(value, out var userId))
        {
            throw new InvalidOperationException("Usuário autenticado sem identificador válido no token.");
        }

        return userId;
    }
}
