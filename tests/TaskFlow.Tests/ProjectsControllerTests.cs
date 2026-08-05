using System.Net;
using System.Net.Http.Json;
using TaskFlow.Api.Dtos;

namespace TaskFlow.Tests;

public class ProjectsControllerTests : IClassFixture<TaskFlowWebApplicationFactory>
{
    private readonly TaskFlowWebApplicationFactory _factory;

    public ProjectsControllerTests(TaskFlowWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_SemToken_Retorna401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_ComTokenValido_Retorna201EApareceNaListagem()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var createResponse = await client.PostAsJsonAsync("/api/projects",
            new ProjectRequest("Projeto Alpha", "Descrição do projeto"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(created);
        Assert.Equal("Projeto Alpha", created!.Name);
        Assert.Equal(0, created.TaskCount);

        var listResponse = await client.GetAsync("/api/projects");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var projects = await listResponse.Content.ReadFromJsonAsync<List<ProjectResponse>>();
        Assert.Contains(projects!, p => p.Id == created.Id);
    }

    [Fact]
    public async Task GetById_DeProjetoDeOutroUsuario_Retorna404()
    {
        var ownerClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "dono");
        var createResponse = await ownerClient.PostAsJsonAsync("/api/projects",
            new ProjectRequest("Projeto Privado", null));
        var project = await createResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var otherClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "outro");
        var response = await otherClient.GetAsync($"/api/projects/{project!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ProjetoExistente_Retorna204ESomeDaListagem()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);
        var createResponse = await client.PostAsJsonAsync("/api/projects",
            new ProjectRequest("Projeto a Remover", null));
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var deleteResponse = await client.DeleteAsync($"/api/projects/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/projects/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
