using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Security;
using TaskFlow.UnitTests.Application;
using Xunit;

namespace TaskFlow.UnitTests.Infrastructure;

public class JwtTokenServiceTests
{
    private static readonly DateTime Now = new(2026, 07, 24, 12, 0, 0, DateTimeKind.Utc);

    private static readonly JwtSettings Settings = new()
    {
        Issuer = "TaskFlow",
        Audience = "TaskFlow.Client",
        SigningKey = "unit-test-signing-key-that-is-definitely-long-enough",
        ExpiryMinutes = 60
    };

    [Fact]
    public void CreateToken_embeds_user_claims_and_expiry()
    {
        var sut = new JwtTokenService(Settings, new FakeTimeProvider(Now));
        var user = new User { Id = Guid.NewGuid(), Email = "u@example.com", DisplayName = "User" };

        var (token, expiresAtUtc) = sut.CreateToken(user);

        expiresAtUtc.Should().Be(Now.AddMinutes(60));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("TaskFlow");
        jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "u@example.com");
        jwt.Claims.Should().Contain(c => c.Type == "name" && c.Value == "User");
    }
}
