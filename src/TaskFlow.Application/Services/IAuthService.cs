using TaskFlow.Application.Common;
using TaskFlow.Application.Contracts;

namespace TaskFlow.Application.Services;

/// <summary>
/// Business operations for user registration and authentication.
/// Implemented in Iteration 3 (see docs/PLAN.md).
/// </summary>
public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
