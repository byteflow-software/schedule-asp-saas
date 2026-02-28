using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scheduly.Application.Features.Tenants.Commands.UpdateTenant;
using Scheduly.Application.Features.Tenants.Commands.ValidateAsaasKey;
using Scheduly.Application.Features.Tenants.Queries.GetTenant;

namespace Scheduly.Api.Controllers;

[Authorize]
public class TenantsController : ApiControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentTenant()
    {
        var result = await Mediator.Send(new GetTenantQuery());
        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentTenant([FromBody] UpdateTenantCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("me/validate-asaas")]
    public async Task<IActionResult> ValidateAsaasKey([FromBody] ValidateAsaasKeyCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
