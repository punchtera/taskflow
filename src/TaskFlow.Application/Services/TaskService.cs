using TaskFlow.Application.Common;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.Validation;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Application.Services;

/// <summary>
/// Task business logic: validation, entity mapping and server-owned timestamps.
/// Depends only on the Domain repository abstraction and a <see cref="TimeProvider"/>,
/// keeping it free of any data-access or web concerns.
/// </summary>
public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly TimeProvider _clock;

    public TaskService(ITaskRepository repository, TimeProvider clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<Result<IReadOnlyList<TaskResponse>>> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var items = await _repository.GetAllForUserAsync(userId, ct);
        IReadOnlyList<TaskResponse> mapped = items.Select(Map).ToList();
        return Result.Success(mapped);
    }

    public async Task<Result<TaskResponse>> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var task = await _repository.GetByIdForUserAsync(id, userId, ct);
        return task is null
            ? Result.Failure<TaskResponse>(Error.NotFound("Task not found."))
            : Result.Success(Map(task));
    }

    public async Task<Result<TaskResponse>> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken ct = default)
    {
        var now = _clock.GetUtcNow().UtcDateTime;

        var validation = TaskRules.Validate(request, now);
        if (validation.IsFailure)
            return Result.Failure<TaskResponse>(validation.Error!);

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Status = request.Status,
            DueDateUtc = request.DueDateUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await _repository.AddAsync(task, ct);
        return Result.Success(Map(task));
    }

    public async Task<Result<TaskResponse>> UpdateAsync(Guid id, Guid userId, UpdateTaskRequest request, CancellationToken ct = default)
    {
        var now = _clock.GetUtcNow().UtcDateTime;

        var validation = TaskRules.Validate(request, now);
        if (validation.IsFailure)
            return Result.Failure<TaskResponse>(validation.Error!);

        var task = await _repository.GetByIdForUserAsync(id, userId, ct);
        if (task is null)
            return Result.Failure<TaskResponse>(Error.NotFound("Task not found."));

        task.Title = request.Title.Trim();
        task.Description = request.Description;
        task.Status = request.Status;
        task.DueDateUtc = request.DueDateUtc;
        task.UpdatedAtUtc = now;

        var updated = await _repository.UpdateAsync(task, ct);
        return updated
            ? Result.Success(Map(task))
            : Result.Failure<TaskResponse>(Error.NotFound("Task not found."));
    }

    public async Task<Result> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var deleted = await _repository.DeleteAsync(id, userId, ct);
        return deleted
            ? Result.Success()
            : Result.Failure(Error.NotFound("Task not found."));
    }

    private static TaskResponse Map(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Status, t.DueDateUtc, t.CreatedAtUtc, t.UpdatedAtUtc);
}
