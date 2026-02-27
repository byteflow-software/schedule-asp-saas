using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
