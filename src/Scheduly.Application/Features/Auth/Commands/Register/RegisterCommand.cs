using MediatR;
using Scheduly.Application.Common.Models;

namespace Scheduly.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string TenantName,
    string FullName,
    string Email,
    string Password) : IRequest<RegisterResult>;

public record RegisterResult(
    Guid TenantId,
    Guid UserId,
    TokenResponse Tokens);
