using System.Net;
using System.Net.Http.Json;
using TaskFlow.Api.Dtos;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Tests;

public class TasksControllerTests : IClassFixture<TaskFlowWebApplicationFactory>
{
    private readonly TaskFlowWebApplicationFactory _factory;

    public TasksControllerTests(TaskFlowWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient Client, int ProjectId)> CreateClientWithProjectAsync()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);
        var projectResponse = await client.PostAsJsonAsync("/api/projects", new ProjectRequest("Projeto com Tarefas", null));
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        return (client, project!.Id);
    }

    [Fact]
    public async Task Create_TarefaEmProjetoValido_Retorna201()
    {
        var (client, projectId) = await CreateClientWithProjectAsync();

        var response = await client.PostAsJsonAsync($"/api/projects/{projectId}/tasks",
            new TaskItemRequest("Implementar login", "Usar JWT", TaskItemStatus.Pendente, null));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var task = await response.Content.ReadFromJsonAsync<TaskItemResponse>();
        Assert.Equal("Implementar login", task!.Title);
        Assert.Equal(projectId, task.ProjectId);
    }

    [Fact]
    public async Task Create_TarefaEmProjetoInexistente_Retorna404()
    {
        var client = await AuthHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.PostAsJsonAsync("/api/projects/999999/tasks",
            new TaskItemRequest("Tarefa órfã", null, TaskItemStatus.Pendente, null));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_AlterandoStatus_RefleteNaConsulta()
    {
        var (client, projectId) = await CreateClientWithProjectAsync();

        var createResponse = await client.PostAsJsonAsync($"/api/projects/{projectId}/tasks",
            new TaskItemRequest("Escrever testes", null, TaskItemStatus.Pendente, null));
        var task = await createResponse.Content.ReadFromJsonAsync<TaskItemResponse>();

        var updateResponse = await client.PutAsJsonAsync($"/api/projects/{projectId}/tasks/{task!.Id}",
            new TaskItemRequest("Escrever testes", "Concluído em code review", TaskItemStatus.Concluida, null));
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/projects/{projectId}/tasks/{task.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<TaskItemResponse>();
        Assert.Equal(TaskItemStatus.Concluida, updated!.Status);
    }

    [Fact]
    public async Task GetAll_DeProjetoDeOutroUsuario_Retorna404()
    {
        var (_, projectId) = await CreateClientWithProjectAsync();
        var otherClient = await AuthHelper.CreateAuthenticatedClientAsync(_factory, "intruso");

        var response = await otherClient.GetAsync($"/api/projects/{projectId}/tasks");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
