using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace TaskFlow.Tests;

/// <summary>
/// Sobe a API inteira em memória (WebApplicationFactory) apontando para um arquivo
/// SQLite temporário isolado por instância, para testes de integração ponta a ponta.
/// </summary>
public class TaskFlowWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"taskflow-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                ["Jwt:Key"] = "test-integration-secret-key-please-do-not-use-in-prod-0123456789",
                ["Jwt:Issuer"] = "TaskFlow.Api.Tests",
                ["Jwt:Audience"] = "TaskFlow.Client.Tests",
                ["Jwt:ExpiresMinutes"] = "120"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (File.Exists(_dbPath))
        {
            try
            {
                File.Delete(_dbPath);
            }
            catch (IOException)
            {
                // Arquivo pode ainda estar em uso por um handle do SQLite; não é crítico em testes.
            }
        }
    }
}
