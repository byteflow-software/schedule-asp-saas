namespace Scheduly.Infrastructure.Services;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string From { get; set; } = "noreply@scheduly.com";
    public string FromName { get; set; } = "Scheduly";
    public bool EnableSsl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
