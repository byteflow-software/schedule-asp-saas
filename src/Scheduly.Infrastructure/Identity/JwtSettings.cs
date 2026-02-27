namespace Scheduly.Infrastructure.Identity;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "Scheduly";
    public string Audience { get; set; } = "Scheduly";
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
