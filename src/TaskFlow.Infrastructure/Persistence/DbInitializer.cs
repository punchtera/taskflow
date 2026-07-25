using Dapper;
using TaskFlow.Application.Abstractions;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Persistence;

/// <summary>
/// Creates the schema on startup (idempotent) and seeds demo data/credentials so
/// a reviewer can clone and immediately log in. Real projects would use a proper
/// migration tool; a startup initializer keeps the demo friction-free.
/// </summary>
public sealed class DbInitializer
{
    private readonly ISqlConnectionFactory _factory;
    private readonly IPasswordHasher _passwordHasher;

    // Stable IDs so seeding is deterministic and repeatable.
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public const string DemoEmail = "demo@taskflow.dev";
    public const string DemoPassword = "Passw0rd!";

    public DbInitializer(ISqlConnectionFactory factory, IPasswordHasher passwordHasher)
    {
        _factory = factory;
        _passwordHasher = passwordHasher;
    }

    public void Initialize()
    {
        using var db = _factory.Create();

        db.Execute("""
            CREATE TABLE IF NOT EXISTS Users (
                Id            TEXT PRIMARY KEY,
                Email         TEXT NOT NULL UNIQUE COLLATE NOCASE,
                DisplayName   TEXT NOT NULL,
                PasswordHash  TEXT NOT NULL,
                CreatedAtUtc  TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Tasks (
                Id            TEXT PRIMARY KEY,
                UserId        TEXT NOT NULL,
                Title         TEXT NOT NULL,
                Description   TEXT NULL,
                Status        INTEGER NOT NULL,
                DueDateUtc    TEXT NULL,
                CreatedAtUtc  TEXT NOT NULL,
                UpdatedAtUtc  TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
            );

            CREATE INDEX IF NOT EXISTS IX_Tasks_UserId ON Tasks(UserId);
            """);

        Seed(db);
    }

    private void Seed(System.Data.IDbConnection db)
    {
        var userExists = db.ExecuteScalar<long>(
            "SELECT COUNT(1) FROM Users WHERE Id = @Id", new { Id = DemoUserId.ToString() }) > 0;

        if (userExists)
            return;

        var now = DateTime.UtcNow;

        db.Execute("""
            INSERT INTO Users (Id, Email, DisplayName, PasswordHash, CreatedAtUtc)
            VALUES (@Id, @Email, @DisplayName, @PasswordHash, @CreatedAtUtc);
            """,
            new
            {
                Id = DemoUserId.ToString(),
                Email = DemoEmail,
                DisplayName = "Demo User",
                PasswordHash = _passwordHasher.Hash(DemoPassword),
                CreatedAtUtc = now.ToString("O")
            });

        var seedTasks = new (string Title, string? Description, TaskState Status, DateTime? Due)[]
        {
            ("Welcome to TaskFlow", "This is a seeded demo task. Edit or delete it.", TaskState.Todo, now.AddDays(2)),
            ("Try the filters", "Mark a task in progress and watch the board update.", TaskState.InProgress, now.AddDays(5)),
            ("Read the README", null, TaskState.Done, now.AddDays(-1))
        };

        foreach (var t in seedTasks)
        {
            db.Execute("""
                INSERT INTO Tasks (Id, UserId, Title, Description, Status, DueDateUtc, CreatedAtUtc, UpdatedAtUtc)
                VALUES (@Id, @UserId, @Title, @Description, @Status, @DueDateUtc, @CreatedAtUtc, @UpdatedAtUtc);
                """,
                new
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = DemoUserId.ToString(),
                    t.Title,
                    t.Description,
                    Status = (int)t.Status,
                    DueDateUtc = t.Due?.ToString("O"),
                    CreatedAtUtc = now.ToString("O"),
                    UpdatedAtUtc = now.ToString("O")
                });
        }
    }
}
