using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Tenants.Commands.UpdateTenant;
using Scheduly.Application.Features.Tenants.Commands.ValidateAsaasKey;
using Scheduly.Application.Features.Tenants.Queries.GetTenant;

namespace Scheduly.Api.Controllers;

[Authorize]
public class TenantsController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IAsaasService _asaasService;

    public TenantsController(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        IAsaasService asaasService)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _asaasService = asaasService;
    }

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

    [HttpGet("me/webhook-status")]
    public async Task<IActionResult> GetWebhookStatus(CancellationToken ct)
    {
        var webhookUrl = $"{Request.Scheme}://{Request.Host}/api/webhooks/asaas";

        var tenantId = _currentTenantService.TenantId;
        var tenant = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == tenantId, ct);

        if (tenant == null || string.IsNullOrEmpty(tenant.AsaasApiKey))
        {
            return Ok(new { webhookUrl, configured = false, webhooks = Array.Empty<object>() });
        }

        var result = await _asaasService.ListWebhooksAsync(tenant.AsaasApiKey, ct);

        var configured = result.Data.Any(w =>
            w.Enabled && w.Url.Contains("/api/webhooks/asaas", StringComparison.OrdinalIgnoreCase));

        return Ok(new
        {
            webhookUrl,
            configured,
            webhooks = result.Data.Select(w => new { w.Url, w.Enabled, w.Name })
        });
    }
}
