namespace TaskFlow.Application.Abstractions;

/// <summary>
/// Supplies the id of the user making the current request. Abstracted so the
/// business layer never touches HttpContext. Until authentication lands
/// (Iteration 3) the API provides an implementation that targets the demo user;
/// afterwards it reads the id from the JWT claims.
/// </summary>
public interface ICurrentUserAccessor
{
    Guid UserId { get; }
}
