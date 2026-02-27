using MediatR;
using Scheduly.Application.Features.Services.DTOs;

namespace Scheduly.Application.Features.Services.Commands.UpdateService;

public record UpdateServiceCommand(
    Guid Id,
    string Name,
    string? Description,
    int DurationMinutes,
    int PriceInCents,
    bool IsActive) : IRequest<ServiceDto>;
