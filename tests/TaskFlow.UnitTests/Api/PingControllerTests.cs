using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace TaskFlow.UnitTests.Api;

public class PingControllerTests
{
    [Fact]
    public async Task GET_ping_is_anonymous_and_returns_ok()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient(); // no token

        var response = await client.GetAsync("/api/ping");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("status").GetString().Should().Be("ok");
    }
}
