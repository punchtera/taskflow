using System.Data;
using Microsoft.Data.Sqlite;

namespace TaskFlow.Infrastructure.Persistence;

public sealed class SqliteConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string connectionString)
        => _connectionString = connectionString;

    public IDbConnection Create()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        // Enforce foreign keys (off by default in SQLite).
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }
}
