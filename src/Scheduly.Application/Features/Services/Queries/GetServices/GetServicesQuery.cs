using MediatR;
using Scheduly.Application.Features.Services.DTOs;

namespace Scheduly.Application.Features.Services.Queries.GetServices;

public record GetServicesQuery(bool? ActiveOnly = null) : IRequest<List<ServiceDto>>;
