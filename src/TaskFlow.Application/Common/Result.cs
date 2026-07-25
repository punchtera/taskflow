namespace TaskFlow.Application.Common;

/// <summary>
/// Categorises a failure so the API layer can map it to the right HTTP status
/// without the Application layer knowing anything about HTTP.
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized
}

public record Error(ErrorType Type, string Message)
{
    public static Error Validation(string message) => new(ErrorType.Validation, message);
    public static Error NotFound(string message) => new(ErrorType.NotFound, message);
    public static Error Conflict(string message) => new(ErrorType.Conflict, message);
    public static Error Unauthorized(string message) => new(ErrorType.Unauthorized, message);
}

/// <summary>
/// A lightweight result type so business methods can report success or a typed
/// failure without throwing exceptions for expected outcomes.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, Error? error) : base(isSuccess, error)
        => _value = value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");
}
