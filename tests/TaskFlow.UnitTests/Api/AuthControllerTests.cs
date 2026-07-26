using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlow.Application.Contracts;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Persistence;
using Xunit;

namespace TaskFlow.UnitTests.Api;

public class AuthControllerTests
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task Register_creates_account_and_returns_201_with_token()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("alice@example.com", "Alice", "Passw0rd!"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(Json);
        auth!.Token.Should().NotBeNullOrWhiteSpace();
        auth.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task Register_with_existing_email_returns_409()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(DbInitializer.DemoEmail, "Dupe", "Passw0rd!"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_with_seeded_demo_credentials_returns_200()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(DbInitializer.DemoEmail, DbInitializer.DemoPassword));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(Json);
        auth!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(DbInitializer.DemoEmail, "wrong-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_without_token_returns_401()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_with_token_returns_the_callers_identity()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        await AuthTestHelper.AuthenticateAsDemoAsync(client);

        var response = await client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("email").GetString().Should().Be(DbInitializer.DemoEmail);
    }

    [Fact]
    public async Task A_new_user_cannot_see_another_users_tasks()
    {
        using var factory = new CustomWebApplicationFactory();

        // Register a brand-new user and use their token.
        var client = factory.CreateClient();
        var register = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("fresh@example.com", "Fresh", "Passw0rd!"));
        var auth = await register.Content.ReadFromJsonAsync<AuthResponse>(Json);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // The new user has no tasks (the seed belongs to the demo user).
        var tasks = await client.GetFromJsonAsync<List<TaskResponse>>("/api/tasks", Json);
        tasks.Should().BeEmpty();

        // Create one, and confirm it belongs to them.
        var created = await (await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Mine", null, TaskState.Todo, null)))
            .Content.ReadFromJsonAsync<TaskResponse>(Json);
        created!.Id.Should().NotBeEmpty();

        var after = await client.GetFromJsonAsync<List<TaskResponse>>("/api/tasks", Json);
        after!.Should().ContainSingle(t => t.Id == created.Id);
    }
}
