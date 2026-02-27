using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scheduly.Application.Features.Customers.Commands.CreateCustomer;
using Scheduly.Application.Features.Customers.Commands.DeleteCustomer;
using Scheduly.Application.Features.Customers.Commands.UpdateCustomer;
using Scheduly.Application.Features.Customers.Queries.GetCustomerById;
using Scheduly.Application.Features.Customers.Queries.GetCustomers;

namespace Scheduly.Api.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class CustomersController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCustomers([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetCustomersQuery(search, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        var result = await Mediator.Send(new GetCustomerByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand command)
    {
        var result = await Mediator.Send(command);
        return Created(string.Empty, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Route id does not match body id." });

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        await Mediator.Send(new DeleteCustomerCommand(id));
        return NoContent();
    }
}
