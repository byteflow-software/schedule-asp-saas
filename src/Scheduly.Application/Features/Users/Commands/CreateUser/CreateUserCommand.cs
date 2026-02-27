using MediatR;
using Scheduly.Application.Features.Users.DTOs;

namespace Scheduly.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string FullName,
    string Email,
    string Password,
    string Role) : IRequest<UserDto>;
