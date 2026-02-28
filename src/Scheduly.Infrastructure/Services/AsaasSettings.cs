namespace Scheduly.Infrastructure.Services;

public class AsaasSettings
{
    public const string SectionName = "Asaas";

    public string BaseUrl { get; set; } = "https://api.asaas.com/v3";
    public string PlatformApiKey { get; set; } = string.Empty;
    public string PlatformWalletId { get; set; } = string.Empty;
    public decimal PlatformSplitPercent { get; set; } = 3;
    public string WebhookToken { get; set; } = string.Empty;
}
