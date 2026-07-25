namespace TaskFlow.Domain.Enums;

/// <summary>
/// Lifecycle status of a task. Persisted as an integer.
/// </summary>
public enum TaskState
{
    Todo = 0,
    InProgress = 1,
    Done = 2
}
