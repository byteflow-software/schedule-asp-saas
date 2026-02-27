using MediatR;
using Scheduly.Application.Common.Models;

namespace Scheduly.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string Token) : IRequest<TokenResponse>;
