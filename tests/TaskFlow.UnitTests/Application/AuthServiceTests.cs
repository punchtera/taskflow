using FluentAssertions;
using Moq;
using TaskFlow.Application.Abstractions;
using TaskFlow.Application.Common;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.Services;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;
using Xunit;

namespace TaskFlow.UnitTests.Application;

public class AuthServiceTests
{
    private static readonly DateTime Now = new(2026, 07, 24, 12, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _tokens.Setup(t => t.CreateToken(It.IsAny<User>()))
               .Returns(("jwt-token", Now.AddHours(1)));
        _sut = new AuthService(_users.Object, _hasher.Object, _tokens.Object, new FakeTimeProvider(Now));
    }

    [Fact]
    public async Task RegisterAsync_creates_user_hashes_password_and_returns_token()
    {
        _users.Setup(u => u.ExistsByEmailAsync("new@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(false);

        var result = await _sut.RegisterAsync(
            new RegisterRequest("new@example.com", "New User", "Passw0rd!"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("jwt-token");
        result.Value.Email.Should().Be("new@example.com");
        _hasher.Verify(h => h.Hash("Passw0rd!"), Times.Once);
        _users.Verify(u => u.AddAsync(
            It.Is<User>(x => x.Email == "new@example.com" && x.PasswordHash == "hashed" && x.CreatedAtUtc == Now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_returns_Conflict_when_email_already_exists()
    {
        _users.Setup(u => u.ExistsByEmailAsync("taken@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(true);

        var result = await _sut.RegisterAsync(
            new RegisterRequest("taken@example.com", "Someone", "Passw0rd!"));

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        _users.Verify(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "User", "Passw0rd!")]        // missing email
    [InlineData("not-an-email", "User", "Passw0rd!")] // malformed email
    [InlineData("a@b.com", "User", "short")]     // weak password
    [InlineData("a@b.com", "", "Passw0rd!")]     // missing display name
    public async Task RegisterAsync_returns_Validation_for_bad_input(string email, string name, string password)
    {
        var result = await _sut.RegisterAsync(new RegisterRequest(email, name, password));

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        _users.Verify(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_returns_Unauthorized_when_user_not_found()
    {
        _users.Setup(u => u.GetByEmailAsync("ghost@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(new LoginRequest("ghost@example.com", "whatever"));

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task LoginAsync_returns_Unauthorized_when_password_is_wrong()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "u@example.com", PasswordHash = "stored" };
        _users.Setup(u => u.GetByEmailAsync("u@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("bad", "stored")).Returns(false);

        var result = await _sut.LoginAsync(new LoginRequest("u@example.com", "bad"));

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Unauthorized);
        _tokens.Verify(t => t.CreateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_returns_token_on_valid_credentials()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "u@example.com", DisplayName = "U", PasswordHash = "stored" };
        _users.Setup(u => u.GetByEmailAsync("u@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Passw0rd!", "stored")).Returns(true);

        var result = await _sut.LoginAsync(new LoginRequest("u@example.com", "Passw0rd!"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("jwt-token");
        result.Value.UserId.Should().Be(user.Id);
    }
}
