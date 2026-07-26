using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.UnitTests.Infrastructure;
using Xunit;

namespace TaskFlow.UnitTests.Infrastructure;

public class UserRepositoryTests : IDisposable
{
    private readonly SqliteTestDatabase _db = new();
    private readonly UserRepository _sut;

    public UserRepositoryTests() => _sut = new UserRepository(_db.Factory);

    public void Dispose() => _db.Dispose();

    private static User NewUser(string email = "alice@example.com") => new()
    {
        Id = Guid.NewGuid(),
        Email = email,
        DisplayName = "Alice",
        PasswordHash = "hash",
        CreatedAtUtc = DateTime.UtcNow
    };

    [Fact]
    public async Task AddAsync_then_GetByIdAsync_roundtrips_the_user()
    {
        var user = NewUser();

        await _sut.AddAsync(user);
        var loaded = await _sut.GetByIdAsync(user.Id);

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(user.Id);
        loaded.Email.Should().Be(user.Email);
        loaded.DisplayName.Should().Be(user.DisplayName);
        loaded.PasswordHash.Should().Be(user.PasswordHash);
        loaded.CreatedAtUtc.Should().BeCloseTo(user.CreatedAtUtc, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_missing()
    {
        var loaded = await _sut.GetByIdAsync(Guid.NewGuid());

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_is_case_insensitive()
    {
        await _sut.AddAsync(NewUser("Bob@Example.com"));

        var loaded = await _sut.GetByEmailAsync("bob@example.com");

        loaded.Should().NotBeNull();
        loaded!.DisplayName.Should().Be("Alice");
    }

    [Fact]
    public async Task ExistsByEmailAsync_reflects_presence()
    {
        await _sut.AddAsync(NewUser("carol@example.com"));

        (await _sut.ExistsByEmailAsync("carol@example.com")).Should().BeTrue();
        (await _sut.ExistsByEmailAsync("nobody@example.com")).Should().BeFalse();
    }
}
