namespace TaskFlow.Infrastructure.Security;

/// <summary>
/// Strongly-typed JWT configuration, bound from the "Jwt" section of appsettings.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 60;
}
