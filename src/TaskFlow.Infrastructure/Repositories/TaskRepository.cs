using Dapper;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Repositories;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly ISqlConnectionFactory _factory;

    public TaskRepository(ISqlConnectionFactory factory) => _factory = factory;

    private const string Columns =
        "Id, UserId, Title, Description, Status, DueDateUtc, CreatedAtUtc, UpdatedAtUtc";

    // Row shape matching the Tasks table, mapped to the domain entity.
    private sealed record TaskRow(
        string Id, string UserId, string Title, string? Description,
        long Status, string? DueDateUtc, string CreatedAtUtc, string UpdatedAtUtc);

    private static TaskItem Map(TaskRow r) => new()
    {
        Id = Guid.Parse(r.Id),
        UserId = Guid.Parse(r.UserId),
        Title = r.Title,
        Description = r.Description,
        Status = (TaskState)r.Status,
        DueDateUtc = SqliteValueConverter.ToNullableDateTime(r.DueDateUtc),
        CreatedAtUtc = SqliteValueConverter.ToDateTime(r.CreatedAtUtc),
        UpdatedAtUtc = SqliteValueConverter.ToDateTime(r.UpdatedAtUtc)
    };

    private static object ToParameters(TaskItem t) => new
    {
        Id = SqliteValueConverter.ToText(t.Id),
        UserId = SqliteValueConverter.ToText(t.UserId),
        t.Title,
        t.Description,
        Status = (long)t.Status,
        DueDateUtc = SqliteValueConverter.ToText(t.DueDateUtc),
        CreatedAtUtc = SqliteValueConverter.ToText(t.CreatedAtUtc),
        UpdatedAtUtc = SqliteValueConverter.ToText(t.UpdatedAtUtc)
    };

    public async Task<IReadOnlyList<TaskItem>> GetAllForUserAsync(Guid userId, CancellationToken ct = default)
    {
        using var db = _factory.Create();
        var rows = await db.QueryAsync<TaskRow>(new CommandDefinition(
            $"SELECT {Columns} FROM Tasks WHERE UserId = @UserId ORDER BY CreatedAtUtc DESC",
            new { UserId = SqliteValueConverter.ToText(userId) }, cancellationToken: ct));
        return rows.Select(Map).ToList();
    }

    public async Task<TaskItem?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        using var db = _factory.Create();
        var row = await db.QuerySingleOrDefaultAsync<TaskRow>(new CommandDefinition(
            $"SELECT {Columns} FROM Tasks WHERE Id = @Id AND UserId = @UserId",
            new { Id = SqliteValueConverter.ToText(id), UserId = SqliteValueConverter.ToText(userId) },
            cancellationToken: ct));
        return row is null ? null : Map(row);
    }

    public async Task AddAsync(TaskItem task, CancellationToken ct = default)
    {
        using var db = _factory.Create();
        await db.ExecuteAsync(new CommandDefinition($"""
            INSERT INTO Tasks ({Columns})
            VALUES (@Id, @UserId, @Title, @Description, @Status, @DueDateUtc, @CreatedAtUtc, @UpdatedAtUtc);
            """, ToParameters(task), cancellationToken: ct));
    }

    public async Task<bool> UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        using var db = _factory.Create();
        var affected = await db.ExecuteAsync(new CommandDefinition("""
            UPDATE Tasks
               SET Title = @Title,
                   Description = @Description,
                   Status = @Status,
                   DueDateUtc = @DueDateUtc,
                   UpdatedAtUtc = @UpdatedAtUtc
             WHERE Id = @Id AND UserId = @UserId;
            """, ToParameters(task), cancellationToken: ct));
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        using var db = _factory.Create();
        var affected = await db.ExecuteAsync(new CommandDefinition(
            "DELETE FROM Tasks WHERE Id = @Id AND UserId = @UserId",
            new { Id = SqliteValueConverter.ToText(id), UserId = SqliteValueConverter.ToText(userId) },
            cancellationToken: ct));
        return affected > 0;
    }
}
