using Scheduly.Application.Common.Models;
using Scheduly.Domain.Entities;

namespace Scheduly.Application.Common.Interfaces;

public interface IJwtTokenService
{
    TokenResponse GenerateTokens(User user);
}
