using Scheduly.Domain.Entities;

namespace Scheduly.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAt)
{
    public static UserDto FromEntity(User user) => new(
        user.Id,
        user.FullName,
        user.Email,
        user.Role.ToString(),
        user.IsActive,
        user.CreatedAt);
}
