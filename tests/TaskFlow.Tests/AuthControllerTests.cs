using System.Net;
using System.Net.Http.Json;
using TaskFlow.Api.Dtos;

namespace TaskFlow.Tests;

public class AuthControllerTests : IClassFixture<TaskFlowWebApplicationFactory>
{
    private readonly TaskFlowWebApplicationFactory _factory;

    public AuthControllerTests(TaskFlowWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ComDadosValidos_Retorna201ComToken()
    {
        var client = _factory.CreateClient();
        var email = $"novo-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            Name: "Fulano da Silva",
            Email: email,
            Password: "Senha@123"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
        Assert.Equal(email, body.Email);
    }

    [Fact]
    public async Task Register_ComEmailDuplicado_Retorna409()
    {
        var client = _factory.CreateClient();
        var email = $"duplicado-{Guid.NewGuid():N}@example.com";
        var request = new RegisterRequest("Primeiro Usuário", email, "Senha@123");

        var first = await client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/auth/register", request with { Name = "Segundo Usuário" });
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_Retorna200ComToken()
    {
        var client = _factory.CreateClient();
        var email = $"login-{Guid.NewGuid():N}@example.com";
        const string password = "Senha@123";

        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Login Teste", email, password));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body!.Token));
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_Retorna401()
    {
        var client = _factory.CreateClient();
        var email = $"senhaerrada-{Guid.NewGuid():N}@example.com";

        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest("Senha Errada", email, "Senha@123"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "SenhaErrada@999"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComEmailInexistente_Retorna401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest($"naoexiste-{Guid.NewGuid():N}@example.com", "Senha@123"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
