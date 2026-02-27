using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RevokeTokenCommandHandler(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken)
            ?? throw new DomainException("INVALID_TOKEN", "Invalid refresh token.");

        if (!token.IsActive)
            throw new DomainException("INVALID_TOKEN", "Token is already revoked or expired.");

        token.RevokedAt = _dateTimeProvider.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
