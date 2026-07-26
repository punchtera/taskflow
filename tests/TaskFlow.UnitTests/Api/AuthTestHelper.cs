using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TaskFlow.Application.Contracts;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.UnitTests.Api;

internal static class AuthTestHelper
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Logs in as the seeded demo user and attaches the bearer token to the client.</summary>
    public static async Task AuthenticateAsDemoAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(DbInitializer.DemoEmail, DbInitializer.DemoPassword));
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(Json);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth!.Token);
    }
}
