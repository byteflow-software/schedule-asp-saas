using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Scheduly.Domain.Exceptions;
using Scheduly.Infrastructure.Services;

namespace Scheduly.UnitTests.Infrastructure.Services;

public class AsaasServiceTests
{
    private readonly AsaasSettings _settings = new()
    {
        PlatformWalletId = "wallet_test",
        PlatformSplitPercent = 3
    };

    [Fact]
    public async Task CreateOrUpdateCustomerAsync_Success_ReturnsResponse()
    {
        var responseJson = JsonSerializer.Serialize(new { id = "cus_123", name = "John", cpfCnpj = "12345678901" });
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateService(handler);

        var result = await sut.CreateOrUpdateCustomerAsync(
            "$aact_hmlg_test", "John", "12345678901", "john@test.com", null, "ext_ref", CancellationToken.None);

        result.Id.Should().Be("cus_123");
        result.Name.Should().Be("John");
    }

    [Fact]
    public async Task CreateOrUpdateCustomerAsync_ApiError_ThrowsInvalidOperation()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, "{\"error\":\"bad request\"}");
        var sut = CreateService(handler);

        var act = () => sut.CreateOrUpdateCustomerAsync(
            "$aact_prod_key", "John", "12345678901", "john@test.com", null, "ext_ref", CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().Where(e => e.Code == "ASAAS_ERROR");
    }

    [Fact]
    public async Task CreatePaymentWithSplitAsync_Success_ReturnsResponse()
    {
        var responseJson = JsonSerializer.Serialize(new
        {
            id = "pay_123", status = "PENDING", billingType = "UNDEFINED",
            invoiceUrl = "https://invoice.test", bankSlipUrl = (string?)null,
            pixQrCodeUrl = "https://pix.test"
        });
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateService(handler);

        var result = await sut.CreatePaymentWithSplitAsync(
            "$aact_hmlg_test", "cus_123", 5000, "Service", "ext_ref", CancellationToken.None);

        result.Id.Should().Be("pay_123");
        result.Status.Should().Be("PENDING");
    }

    [Fact]
    public async Task CreatePaymentWithSplitAsync_NoSplit_WhenSettingsEmpty()
    {
        var settings = new AsaasSettings { PlatformWalletId = "", PlatformSplitPercent = 0 };
        var responseJson = JsonSerializer.Serialize(new
        {
            id = "pay_456", status = "PENDING", billingType = "PIX",
            invoiceUrl = (string?)null, bankSlipUrl = (string?)null, pixQrCodeUrl = (string?)null
        });
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateService(handler, settings);

        var result = await sut.CreatePaymentWithSplitAsync(
            "$aact_prod_key", "cus_123", 5000, "Service", "ext_ref", CancellationToken.None);

        result.Id.Should().Be("pay_456");
    }

    [Fact]
    public async Task CreatePaymentWithSplitAsync_ApiError_Throws()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, "{}");
        var sut = CreateService(handler);

        var act = () => sut.CreatePaymentWithSplitAsync(
            "$aact_prod_key", "cus_123", 5000, "Service", "ext_ref", CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().Where(e => e.Code == "ASAAS_ERROR");
    }

    [Fact]
    public async Task ValidateApiKeyAsync_Success_ReturnsAccountResponse()
    {
        var responseJson = JsonSerializer.Serialize(new { walletId = "wallet_abc", name = "My Account" });
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateService(handler);

        var result = await sut.ValidateApiKeyAsync("$aact_hmlg_test", CancellationToken.None);

        result.WalletId.Should().Be("wallet_abc");
        result.Name.Should().Be("My Account");
    }

    [Fact]
    public async Task ValidateApiKeyAsync_InvalidKey_Throws()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized, "{}");
        var sut = CreateService(handler);

        var act = () => sut.ValidateApiKeyAsync("invalid_key", CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().Where(e => e.Code == "INVALID_ASAAS_KEY");
    }

    [Fact]
    public async Task ValidateApiKeyAsync_FallbackToIdAndCompanyName()
    {
        var responseJson = JsonSerializer.Serialize(new { id = "acc_123", companyName = "My Company" });
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateService(handler);

        var result = await sut.ValidateApiKeyAsync("$aact_hmlg_test", CancellationToken.None);

        result.WalletId.Should().Be("acc_123");
        result.Name.Should().Be("My Company");
    }

    [Fact]
    public async Task SandboxUrl_UsedForHmlgKey()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK,
            JsonSerializer.Serialize(new { walletId = "w", name = "n" }));
        var sut = CreateService(handler);

        await sut.ValidateApiKeyAsync("$aact_hmlg_test", CancellationToken.None);

        handler.LastRequestUri.Should().Contain("sandbox.asaas.com");
    }

    [Fact]
    public async Task ProductionUrl_UsedForProdKey()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK,
            JsonSerializer.Serialize(new { walletId = "w", name = "n" }));
        var sut = CreateService(handler);

        await sut.ValidateApiKeyAsync("$aact_prod_test", CancellationToken.None);

        handler.LastRequestUri.Should().Contain("api.asaas.com");
    }

    private AsaasService CreateService(FakeHttpMessageHandler handler, AsaasSettings? settings = null)
    {
        var httpClient = new HttpClient(handler);
        return new AsaasService(
            httpClient,
            Options.Create(settings ?? _settings),
            NullLogger<AsaasService>.Instance);
    }

    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;
        public string? LastRequestUri { get; private set; }

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
        {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            LastRequestUri = request.RequestUri?.ToString();
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody, System.Text.Encoding.UTF8, "application/json")
            });
        }
    }
}
