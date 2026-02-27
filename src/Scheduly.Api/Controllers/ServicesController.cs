using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scheduly.Application.Features.Services.Commands.CreateService;
using Scheduly.Application.Features.Services.Commands.UpdateService;
using Scheduly.Application.Features.Services.Queries.GetServiceById;
using Scheduly.Application.Features.Services.Queries.GetServices;

namespace Scheduly.Api.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ServicesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetServices([FromQuery] bool? activeOnly)
    {
        var result = await Mediator.Send(new GetServicesQuery(activeOnly));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetServiceById(Guid id)
    {
        var result = await Mediator.Send(new GetServiceByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateService([FromBody] CreateServiceCommand command)
    {
        var result = await Mediator.Send(command);
        return Created(string.Empty, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Route id does not match body id." });

        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
