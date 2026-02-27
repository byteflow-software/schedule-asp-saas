namespace Scheduly.Application.Common.Models;

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt);
