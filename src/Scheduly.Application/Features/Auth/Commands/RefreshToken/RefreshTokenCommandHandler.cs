using MediatR;
using Microsoft.EntityFrameworkCore;
using Scheduly.Application.Common.Interfaces;
using Scheduly.Application.Common.Models;
using Scheduly.Domain.Exceptions;

namespace Scheduly.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters ensures the User navigation is loaded regardless of
        // tenant context (refresh-token requests don't carry X-Tenant-Id).
        var existingToken = await _context.RefreshTokens
            .IgnoreQueryFilters()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.Token, cancellationToken)
            ?? throw new DomainException("INVALID_TOKEN", "Invalid refresh token.");

        if (!existingToken.IsActive)
            throw new DomainException("INVALID_TOKEN", "Refresh token is expired or revoked.");

        existingToken.RevokedAt = _dateTimeProvider.UtcNow;

        var newTokens = _jwtTokenService.GenerateTokens(existingToken.User);

        var newRefreshToken = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = existingToken.UserId,
            Token = newTokens.RefreshToken,
            ExpiresAt = _dateTimeProvider.UtcNow.AddDays(7),
            CreatedAt = _dateTimeProvider.UtcNow,
            CreatedByIp = "system"
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return newTokens;
    }
}
