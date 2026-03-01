using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Scheduly.Application;
using Scheduly.Application.Common.Behaviors;

namespace Scheduly.UnitTests.Application.Common;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersMediatR()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();

        mediator.Should().NotBeNull();
    }

    [Fact]
    public void AddApplication_RegistersValidationBehavior()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();

        var descriptors = services.Where(s =>
            s.ServiceType == typeof(IPipelineBehavior<,>)).ToList();

        descriptors.Should().Contain(d =>
            d.ImplementationType == typeof(ValidationBehavior<,>));
    }

    [Fact]
    public void AddApplication_RegistersLoggingBehavior()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();

        var descriptors = services.Where(s =>
            s.ServiceType == typeof(IPipelineBehavior<,>)).ToList();

        descriptors.Should().Contain(d =>
            d.ImplementationType == typeof(LoggingBehavior<,>));
    }
}
