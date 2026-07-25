using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Repositories;

/// <summary>
/// Persistence contract for <see cref="TaskItem"/>. All reads are scoped by userId
/// so a user can only ever see or mutate their own tasks.
/// </summary>
public interface ITaskRepository
{
    Task<IReadOnlyList<TaskItem>> GetAllForUserAsync(Guid userId, CancellationToken ct = default);
    Task<TaskItem?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task AddAsync(TaskItem task, CancellationToken ct = default);
    Task<bool> UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);
}
