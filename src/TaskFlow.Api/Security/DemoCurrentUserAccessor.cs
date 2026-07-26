using TaskFlow.Application.Abstractions;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Api.Security;

/// <summary>
/// Interim <see cref="ICurrentUserAccessor"/> used before authentication exists
/// (Iteration 3). It resolves to the seeded demo user so the task CRUD flow is
/// fully exercisable end-to-end. Iteration 3 replaces this with a claims-based
/// accessor reading the id from the JWT.
/// </summary>
public sealed class DemoCurrentUserAccessor : ICurrentUserAccessor
{
    public Guid UserId => DbInitializer.DemoUserId;
}
