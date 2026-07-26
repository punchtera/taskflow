using System.Data;
using Dapper;

namespace TaskFlow.Infrastructure.Persistence;

/// <summary>
/// The database schema, kept separate from seeding so it can be created cleanly
/// in tests (a fresh, empty database) as well as at application startup.
/// </summary>
public static class DatabaseSchema
{
    public const string Sql = """
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
        """;

    /// <summary>Creates the schema if it does not already exist (idempotent).</summary>
    public static void EnsureCreated(IDbConnection db) => db.Execute(Sql);
}
