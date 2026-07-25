namespace TaskFlow.Application.Abstractions;

/// <summary>
/// Hashing is an infrastructure concern; the business layer depends only on this
/// abstraction so the algorithm (BCrypt, Argon2, ...) can change without touching services.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
