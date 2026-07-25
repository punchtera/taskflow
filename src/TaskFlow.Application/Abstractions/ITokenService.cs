using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Abstractions;

/// <summary>
/// Issues authentication tokens for a user. Kept as an abstraction so the JWT
/// implementation lives in Infrastructure.
/// </summary>
public interface ITokenService
{
    /// <returns>A signed token and its UTC expiry.</returns>
    (string Token, DateTime ExpiresAtUtc) CreateToken(User user);
}
