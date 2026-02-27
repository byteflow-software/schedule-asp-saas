using MediatR;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Features.Users.DTOs;
using Scheduly.Domain.Entities;
using Scheduly.Domain.Enums;

namespace Scheduly.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateUserCommandHandler(
        IApplicationDbContext context,
        ICurrentTenantService currentTenantService,
        IPasswordHasher passwordHasher,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _currentTenantService = currentTenantService;
        _passwordHasher = passwordHasher;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentTenantService.TenantId;
        var role = Enum.Parse<UserRole>(request.Role, ignoreCase: true);

        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FullName = request.FullName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = role,
            IsActive = true,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return UserDto.FromEntity(user);
    }
}
