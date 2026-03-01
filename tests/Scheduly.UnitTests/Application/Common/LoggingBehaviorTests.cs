using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Scheduly.Application.Common.Behaviors;
using Scheduly.Application.Common.Interfaces;

namespace Scheduly.UnitTests.Application.Common;

public class LoggingBehaviorTests
{
    private readonly ILogger<LoggingBehavior<TestRequest, TestResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICurrentTenantService _currentTenantService;
    private readonly LoggingBehavior<TestRequest, TestResponse> _sut;

    public LoggingBehaviorTests()
    {
        _logger = Substitute.For<ILogger<LoggingBehavior<TestRequest, TestResponse>>>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _currentTenantService = Substitute.For<ICurrentTenantService>();
        _sut = new LoggingBehavior<TestRequest, TestResponse>(_logger, _currentUserService, _currentTenantService);
    }

    [Fact]
    public async Task Handle_ShouldCallNextAndReturnResponse()
    {
        var request = new TestRequest();
        var expectedResponse = new TestResponse { Value = "OK" };
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        _currentUserService.UserId.Returns(userId);
        _currentTenantService.TenantId.Returns(tenantId);

        var result = await _sut.Handle(request,
            ct => Task.FromResult(expectedResponse),
            CancellationToken.None);

        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_ShouldLogRequestName()
    {
        _currentUserService.UserId.Returns((Guid?)null);
        _currentTenantService.TenantId.Returns(Guid.Empty);

        await _sut.Handle(new TestRequest(),
            ct => Task.FromResult(new TestResponse()),
            CancellationToken.None);

        _logger.ReceivedCalls().Should().HaveCountGreaterThanOrEqualTo(2);
    }

    public record TestRequest : IRequest<TestResponse>;
    public class TestResponse { public string Value { get; set; } = ""; }
}
