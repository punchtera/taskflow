using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using TaskFlow.Application.Abstractions;

namespace TaskFlow.Api.Security;

/// <summary>
/// Resolves the current user's id from the authenticated principal's "sub" claim
/// (populated by the JWT bearer handler). Used on <c>[Authorize]</c> endpoints,
/// so a principal is always present when this is read.
/// </summary>
public sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public Guid UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var value = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                        ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id)
                ? id
                : throw new InvalidOperationException("No authenticated user id on the request.");
        }
    }
}
