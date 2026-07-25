using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Abstractions;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Security;

namespace TaskFlow.Infrastructure;

/// <summary>
/// Single entry point for wiring the Infrastructure layer. The API composition
/// root calls this so it never references concrete data-access types directly.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=taskflow.db";

        services.AddSingleton<ISqlConnectionFactory>(new SqliteConnectionFactory(connectionString));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<DbInitializer>();

        // Repositories (ITaskRepository, IUserRepository) and ITokenService are
        // registered here as they are implemented in Iterations 2 and 3.

        return services;
    }
}
