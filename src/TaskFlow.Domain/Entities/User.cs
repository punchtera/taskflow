namespace TaskFlow.Domain.Entities;

/// <summary>
/// Application user. Passwords are never stored in plain text; only the hash is kept.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
