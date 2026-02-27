using MediatR;
using Scheduly.Application.Features.Services.DTOs;

namespace Scheduly.Application.Features.Services.Commands.CreateService;

public record CreateServiceCommand(
    string Name,
    string? Description,
    int DurationMinutes,
    int PriceInCents) : IRequest<ServiceDto>;
