using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scheduly.Application.Features.Appointments.Commands.CancelAppointment;
using Scheduly.Application.Features.Appointments.Commands.CompleteAppointment;
using Scheduly.Application.Features.Appointments.Commands.ConfirmAppointment;
using Scheduly.Application.Features.Appointments.Commands.CreateAppointment;
using Scheduly.Application.Features.Appointments.Commands.UpdateAppointment;
using Scheduly.Application.Features.Appointments.Queries.GetAppointmentById;
using Scheduly.Application.Features.Appointments.Queries.GetAppointments;

namespace Scheduly.Api.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class AppointmentsController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? userId,
        [FromQuery] Guid? customerId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await Mediator.Send(new GetAppointmentsQuery(from, to, userId, customerId, pageNumber, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAppointmentById(Guid id)
    {
        var result = await Mediator.Send(new GetAppointmentByIdQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentCommand command)
    {
        var result = await Mediator.Send(command);
        return Created(string.Empty, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] UpdateAppointmentRequest request)
    {
        var result = await Mediator.Send(new UpdateAppointmentCommand(id, request.StartTime, request.EndTime, request.Notes));
        return Ok(result);
    }

    [HttpPatch("{id:guid}/confirm")]
    public async Task<IActionResult> ConfirmAppointment(Guid id)
    {
        await Mediator.Send(new ConfirmAppointmentCommand(id));
        return NoContent();
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelAppointment(Guid id)
    {
        await Mediator.Send(new CancelAppointmentCommand(id));
        return NoContent();
    }

    [HttpPatch("{id:guid}/done")]
    public async Task<IActionResult> CompleteAppointment(Guid id)
    {
        await Mediator.Send(new CompleteAppointmentCommand(id));
        return NoContent();
    }
}

public record UpdateAppointmentRequest(DateTime StartTime, DateTime EndTime, string? Notes);
