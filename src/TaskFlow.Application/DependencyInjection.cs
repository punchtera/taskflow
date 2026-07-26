using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Services;

namespace TaskFlow.Application;

/// <summary>
/// Registers the business (Application) layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
