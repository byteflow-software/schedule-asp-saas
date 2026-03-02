namespace Scheduly.Application.Common.Models.Asaas;

public record AsaasWebhookListResponse(List<AsaasWebhookItem> Data);

public record AsaasWebhookItem(string Url, bool Enabled, string? Name);
