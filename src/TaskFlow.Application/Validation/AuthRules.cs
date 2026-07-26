using TaskFlow.Application.Common;
using TaskFlow.Application.Contracts;

namespace TaskFlow.Application.Validation;

/// <summary>
/// Pure validation rules for registration, kept separate for easy unit testing.
/// </summary>
public static class AuthRules
{
    public const int MinPasswordLength = 8;

    public static Result ValidateRegister(RegisterRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Email) || !IsValidEmail(r.Email))
            return Result.Failure(Error.Validation("A valid email is required."));

        if (string.IsNullOrWhiteSpace(r.DisplayName))
            return Result.Failure(Error.Validation("Display name is required."));

        if (string.IsNullOrEmpty(r.Password) || r.Password.Length < MinPasswordLength)
            return Result.Failure(Error.Validation($"Password must be at least {MinPasswordLength} characters."));

        return Result.Success();
    }

    private static bool IsValidEmail(string email)
    {
        var at = email.IndexOf('@');
        return at > 0 && at < email.Length - 1;
    }
}
