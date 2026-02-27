using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scheduly.Application.Features.Users.Commands.CreateUser;
using Scheduly.Application.Features.Users.Commands.DeactivateUser;
using Scheduly.Application.Features.Users.Queries.GetUsers;

namespace Scheduly.Api.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var result = await Mediator.Send(new GetUsersQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        return Created(string.Empty, result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        await Mediator.Send(new DeactivateUserCommand(id));
        return NoContent();
    }
}
