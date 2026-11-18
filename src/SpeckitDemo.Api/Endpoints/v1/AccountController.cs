using System.Security.Claims;
using SpeckitDemo.Application.Abstractions;
using SpeckitDemo.Contracts.Envelope;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SpeckitDemo.Api.Endpoints.v1;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/users")]
public sealed class AccountController : ControllerBase
{
    private readonly IAccountLookup _accounts;

    public AccountController(IAccountLookup accounts)
    {
        _accounts = accounts;
    }

    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var subject = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (subject is null || !int.TryParse(subject, out var userId))
        {
            return Unauthorized(ResultEnvelope<object>.Fail("Authentication required"));
        }

        var result = await _accounts.GetCurrentUserAsync(userId);
        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }
}
