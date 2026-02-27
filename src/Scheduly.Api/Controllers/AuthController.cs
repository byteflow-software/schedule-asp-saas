using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scheduly.Application.Features.Auth.Commands.Login;
using Scheduly.Application.Features.Auth.Commands.Register;
using Scheduly.Application.Features.Auth.Commands.RefreshToken;
using Scheduly.Application.Features.Auth.Commands.RevokeToken;

namespace Scheduly.Api.Controllers;

[AllowAnonymous]
public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await Mediator.Send(command);
        return Created(string.Empty, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
}
