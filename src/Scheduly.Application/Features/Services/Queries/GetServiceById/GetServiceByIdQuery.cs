using MediatR;
using Scheduly.Application.Features.Services.DTOs;

namespace Scheduly.Application.Features.Services.Queries.GetServiceById;

public record GetServiceByIdQuery(Guid Id) : IRequest<ServiceDto>;
