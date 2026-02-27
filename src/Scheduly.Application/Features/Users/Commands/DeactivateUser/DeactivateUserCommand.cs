using MediatR;

namespace Scheduly.Application.Features.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid UserId) : IRequest;
