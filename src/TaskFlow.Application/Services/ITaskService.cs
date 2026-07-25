using TaskFlow.Application.Common;
using TaskFlow.Application.Contracts;

namespace TaskFlow.Application.Services;

/// <summary>
/// Business operations for task CRUD, always scoped to the authenticated user.
/// Implemented in Iteration 2 (see docs/PLAN.md).
/// </summary>
public interface ITaskService
{
    Task<Result<IReadOnlyList<TaskResponse>>> GetAllAsync(Guid userId, CancellationToken ct = default);
    Task<Result<TaskResponse>> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<Result<TaskResponse>> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken ct = default);
    Task<Result<TaskResponse>> UpdateAsync(Guid id, Guid userId, UpdateTaskRequest request, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
}
