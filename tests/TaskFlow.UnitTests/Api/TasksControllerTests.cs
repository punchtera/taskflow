using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlow.Application.Contracts;
using TaskFlow.Domain.Enums;
using Xunit;

namespace TaskFlow.UnitTests.Api;

/// <summary>
/// Integration tests over the running API. Each test uses a fresh factory (and
/// therefore a fresh seeded database) for isolation.
/// </summary>
public class TasksControllerTests
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task GET_tasks_returns_the_seeded_tasks()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskResponse>>(Json);
        tasks.Should().NotBeNull();
        tasks!.Should().HaveCountGreaterThanOrEqualTo(3); // demo seed
    }

    [Fact]
    public async Task POST_task_creates_it_and_returns_201_with_location()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var request = new CreateTaskRequest("Buy milk", "2%", TaskState.Todo, DateTime.UtcNow.AddDays(1));

        var response = await client.PostAsJsonAsync("/api/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var created = await response.Content.ReadFromJsonAsync<TaskResponse>(Json);
        created!.Id.Should().NotBeEmpty();
        created.Title.Should().Be("Buy milk");

        // And it is retrievable.
        var fetched = await client.GetAsync($"/api/tasks/{created.Id}");
        fetched.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task POST_task_with_blank_title_returns_400()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var request = new CreateTaskRequest("", null, TaskState.Todo, null);

        var response = await client.PostAsJsonAsync("/api/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GET_task_by_unknown_id_returns_404()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PUT_then_GET_reflects_the_update()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Draft", null, TaskState.Todo, null)))
            .Content.ReadFromJsonAsync<TaskResponse>(Json);

        var update = new UpdateTaskRequest("Final", "done", TaskState.Done, null);
        var putResponse = await client.PutAsJsonAsync($"/api/tasks/{created!.Id}", update);

        putResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await putResponse.Content.ReadFromJsonAsync<TaskResponse>(Json);
        updated!.Title.Should().Be("Final");
        updated.Status.Should().Be(TaskState.Done);
    }

    [Fact]
    public async Task DELETE_removes_the_task()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var created = await (await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Temp", null, TaskState.Todo, null)))
            .Content.ReadFromJsonAsync<TaskResponse>(Json);

        var deleteResponse = await client.DeleteAsync($"/api/tasks/{created!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/tasks/{created.Id}")).StatusCode
            .Should().Be(HttpStatusCode.NotFound);
    }
}
