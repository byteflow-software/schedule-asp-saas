using MediatR;
using Scheduly.Application.Features.Users.DTOs;

namespace Scheduly.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<List<UserDto>>;
