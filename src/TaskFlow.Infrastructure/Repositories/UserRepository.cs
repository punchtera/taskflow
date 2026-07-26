using Dapper;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _factory;

    public UserRepository(ISqlConnectionFactory factory) => _factory = factory;

    // Row shape matching the Users table (all TEXT columns), mapped to the domain entity.
    private sealed record UserRow(string Id, string Email, string DisplayName, string PasswordHash, string CreatedAtUtc);

    private static User Map(UserRow r) => new()
    {
        Id = Guid.Parse(r.Id),
        Email = r.Email,
        DisplayName = r.DisplayName,
        PasswordHash = r.PasswordHash,
        CreatedAtUtc = SqliteValueConverter.ToDateTime(r.CreatedAtUtc)
    };

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var db = _factory.Create();
        var row = await db.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(
                "SELECT Id, Email, DisplayName, PasswordHash, CreatedAtUtc FROM Users WHERE Id = @Id",
                new { Id = SqliteValueConverter.ToText(id) }, cancellationToken: ct));
        return row is null ? null : Map(row);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        using var db = _factory.Create();
        var row = await db.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(
                "SELECT Id, Email, DisplayName, PasswordHash, CreatedAtUtc FROM Users WHERE Email = @Email",
                new { Email = email }, cancellationToken: ct));
        return row is null ? null : Map(row);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        using var db = _factory.Create();
        var count = await db.ExecuteScalarAsync<long>(
            new CommandDefinition(
                "SELECT COUNT(1) FROM Users WHERE Email = @Email",
                new { Email = email }, cancellationToken: ct));
        return count > 0;
    }

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        using var db = _factory.Create();
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO Users (Id, Email, DisplayName, PasswordHash, CreatedAtUtc)
            VALUES (@Id, @Email, @DisplayName, @PasswordHash, @CreatedAtUtc);
            """,
            new
            {
                Id = SqliteValueConverter.ToText(user.Id),
                user.Email,
                user.DisplayName,
                user.PasswordHash,
                CreatedAtUtc = SqliteValueConverter.ToText(user.CreatedAtUtc)
            }, cancellationToken: ct));
    }
}
