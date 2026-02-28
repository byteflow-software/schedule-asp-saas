using MediatR;
using Scheduly.Application.Features.Tenants.DTOs;

namespace Scheduly.Application.Features.Tenants.Commands.ValidateAsaasKey;

public record ValidateAsaasKeyCommand(string ApiKey) : IRequest<TenantDto>;
