using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Configuration;
using TaskFlow.Api.Services;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Tests;

public class TokenServiceTests
{
    private static ITokenService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "unit-test-secret-key-0123456789-0123456789",
                ["Jwt:Issuer"] = "TaskFlow.Api.Tests",
                ["Jwt:Audience"] = "TaskFlow.Client.Tests",
                ["Jwt:ExpiresMinutes"] = "60"
            })
            .Build();

        return new TokenService(configuration);
    }

    [Fact]
    public void GenerateToken_RetornaJwtValidoComClaimsDoUsuario()
    {
        var service = CreateService();
        var user = new AppUser { Id = 42, Name = "Ana Teste", Email = "ana@example.com" };

        var (token, expiresAtUtc) = service.GenerateToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAtUtc > DateTime.UtcNow);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("42", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("ana@example.com", jwt.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("TaskFlow.Api.Tests", jwt.Issuer);
    }

    [Fact]
    public void GenerateToken_SemChaveConfigurada_LancaExcecao()
    {
        var configuration = new ConfigurationBuilder().Build();
        var service = new TokenService(configuration);

        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(new AppUser { Id = 1 }));
    }
}
