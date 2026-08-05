using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskFlow.Api.Dtos;

namespace TaskFlow.Tests;

internal static class AuthHelper
{
    /// <summary>
    /// Registra um usuário com dados únicos e retorna um HttpClient já autenticado
    /// (header Authorization: Bearer preenchido), pronto para chamar endpoints protegidos.
    /// </summary>
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        TaskFlowWebApplicationFactory factory, string? emailPrefix = null)
    {
        var client = factory.CreateClient();

        var email = $"{emailPrefix ?? "user"}-{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            Name: "Usuário de Teste",
            Email: email,
            Password: "Senha@123"));

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }
}
