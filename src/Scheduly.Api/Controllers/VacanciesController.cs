using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scheduly.Application.Features.Vacancies.Commands.BulkCreateVacancies;
using Scheduly.Application.Features.Vacancies.Commands.CreateVacancy;
using Scheduly.Application.Features.Vacancies.Commands.DeleteVacancy;
using Scheduly.Application.Features.Vacancies.Queries.GetVacancies;

namespace Scheduly.Api.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class VacanciesController : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetVacancies(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? serviceId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] bool? availableOnly)
    {
        var result = await Mediator.Send(new GetVacanciesQuery(userId, serviceId, from, to, availableOnly));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVacancy([FromBody] CreateVacancyCommand command)
    {
        var result = await Mediator.Send(command);
        return Created(string.Empty, result);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> BulkCreateVacancies([FromBody] BulkCreateVacanciesCommand command)
    {
        var result = await Mediator.Send(command);
        return Created(string.Empty, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVacancy(Guid id)
    {
        await Mediator.Send(new DeleteVacancyCommand(id));
        return NoContent();
    }
}
