using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Api.Controllers;

/// <summary>
/// Liveness endpoint used to confirm the API is up. No auth, no dependencies.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    /// <summary>GET /api/ping -> 200 with a small status payload.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "ok",
        service = "TaskFlow.Api",
        timestampUtc = DateTime.UtcNow
    });
}
