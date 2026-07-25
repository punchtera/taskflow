using System.Data;

namespace TaskFlow.Infrastructure.Persistence;

/// <summary>
/// Creates open ADO.NET connections. Abstracted so repositories don't care which
/// concrete provider is in use and so it can be faked in tests.
/// </summary>
public interface ISqlConnectionFactory
{
    IDbConnection Create();
}
