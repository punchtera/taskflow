using TaskFlow.Application.Abstractions;
using TaskFlow.Application.Common;
using TaskFlow.Application.Contracts;
using TaskFlow.Application.Validation;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Repositories;

namespace TaskFlow.Application.Services;

/// <summary>
/// Registration and authentication logic. Passwords are only ever stored as
/// hashes, and login failures are deliberately generic so they don't reveal
/// whether the email or the password was wrong.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly TimeProvider _clock;

    public AuthService(IUserRepository users, IPasswordHasher hasher, ITokenService tokens, TimeProvider clock)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
        _clock = clock;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var validation = AuthRules.ValidateRegister(request);
        if (validation.IsFailure)
            return Result.Failure<AuthResponse>(validation.Error!);

        var email = request.Email.Trim();

        if (await _users.ExistsByEmailAsync(email, ct))
            return Result.Failure<AuthResponse>(Error.Conflict("An account with this email already exists."));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            CreatedAtUtc = _clock.GetUtcNow().UtcDateTime
        };

        await _users.AddAsync(user, ct);
        return Result.Success(Issue(user));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByEmailAsync(request.Email.Trim(), ct);
        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResponse>(Error.Unauthorized("Invalid email or password."));

        return Result.Success(Issue(user));
    }

    private AuthResponse Issue(User user)
    {
        var (token, expiresAtUtc) = _tokens.CreateToken(user);
        return new AuthResponse(token, expiresAtUtc, user.Id, user.Email, user.DisplayName);
    }
}
