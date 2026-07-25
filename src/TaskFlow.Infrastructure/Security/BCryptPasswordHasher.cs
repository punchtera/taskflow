using TaskFlow.Application.Abstractions;

namespace TaskFlow.Infrastructure.Security;

/// <summary>
/// BCrypt-based password hashing. The work factor is intentionally left at the
/// library default (11) which is a sensible balance for a demo.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
