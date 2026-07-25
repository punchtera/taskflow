using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Contracts;

public record CreateTaskRequest(string Title, string? Description, TaskState Status, DateTime? DueDateUtc);

public record UpdateTaskRequest(string Title, string? Description, TaskState Status, DateTime? DueDateUtc);

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    TaskState Status,
    DateTime? DueDateUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
