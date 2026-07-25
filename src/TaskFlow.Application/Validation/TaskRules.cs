using TaskFlow.Application.Common;
using TaskFlow.Application.Contracts;

namespace TaskFlow.Application.Validation;

/// <summary>
/// Pure, side-effect-free business rules for tasks. Kept separate from the
/// service so the rules are trivially unit-testable (see TaskRulesTests) and
/// reused by both Create and Update paths.
/// </summary>
public static class TaskRules
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    /// <summary>
    /// Validates the shared fields of a task write request.
    /// Returns a validation <see cref="Error"/> or success.
    /// </summary>
    public static Result Validate(string title, string? description, DateTime? dueDateUtc, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(Error.Validation("Title is required."));

        if (title.Length > TitleMaxLength)
            return Result.Failure(Error.Validation($"Title must be {TitleMaxLength} characters or fewer."));

        if (description is { Length: > DescriptionMaxLength })
            return Result.Failure(Error.Validation($"Description must be {DescriptionMaxLength} characters or fewer."));

        if (dueDateUtc is { } due && due.Date < nowUtc.Date)
            return Result.Failure(Error.Validation("Due date cannot be in the past."));

        return Result.Success();
    }

    public static Result Validate(CreateTaskRequest r, DateTime nowUtc)
        => Validate(r.Title, r.Description, r.DueDateUtc, nowUtc);

    public static Result Validate(UpdateTaskRequest r, DateTime nowUtc)
        => Validate(r.Title, r.Description, r.DueDateUtc, nowUtc);
}
