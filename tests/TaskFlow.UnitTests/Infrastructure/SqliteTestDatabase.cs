using Microsoft.Data.Sqlite;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.UnitTests.Infrastructure;

/// <summary>
/// Spins up a throwaway, on-disk SQLite database with the real schema (no seed
/// data) for repository tests, then deletes it on dispose. Using a temp file —
/// rather than :memory: — means the same database survives the multiple
/// short-lived connections that the connection factory opens per call.
/// </summary>
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly string _path;

    public ISqlConnectionFactory Factory { get; }

    public SqliteTestDatabase()
    {
        _path = Path.Combine(Path.GetTempPath(), $"taskflow-test-{Guid.NewGuid():N}.db");
        Factory = new SqliteConnectionFactory($"Data Source={_path}");

        using var db = Factory.Create();
        DatabaseSchema.EnsureCreated(db);
    }

    public void Dispose()
    {
        // Release pooled connections so the file handle is freed before deletion.
        SqliteConnection.ClearAllPools();
        if (File.Exists(_path))
            File.Delete(_path);
    }
}
