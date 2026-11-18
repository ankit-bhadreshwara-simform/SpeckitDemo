using SpeckitDemo.Application.Abstractions;
using SpeckitDemo.Contracts.Inbound;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace SpeckitDemo.Api.Endpoints.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class SignInController : ControllerBase
{
    private readonly ISignInWorkflow _signIn;

    public SignInController(ISignInWorkflow signIn)
    {
        _signIn = signIn;
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] CredentialPayload request)
    {
        var result = await _signIn.LoginAsync(request);
        if (result.Success)
        {
            return Ok(result);
        }

        if (result.Message == "Validation failed")
        {
            return BadRequest(result);
        }

        return Unauthorized(result);
    }
}
