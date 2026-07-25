namespace TaskFlow.Application.Contracts;

public record RegisterRequest(string Email, string DisplayName, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, DateTime ExpiresAtUtc, Guid UserId, string Email, string DisplayName);
