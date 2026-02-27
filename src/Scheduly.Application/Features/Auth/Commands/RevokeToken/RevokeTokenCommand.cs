using MediatR;

namespace Scheduly.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string Token) : IRequest;
