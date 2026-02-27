using MediatR;
using Scheduly.Application.Common.Models;

namespace Scheduly.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password) : IRequest<LoginResult>;

public record LoginResult(
    Guid UserId,
    Guid TenantId,
    string FullName,
    string Role,
    TokenResponse Tokens);
