using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Scheduly.Application.Common.Behaviors;
using ValidationException = Scheduly.Application.Common.Exceptions.ValidationException;

namespace Scheduly.UnitTests.Application.Common;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var sut = new ValidationBehavior<TestRequest, string>(
            Enumerable.Empty<IValidator<TestRequest>>());

        var result = await sut.Handle(new TestRequest("test"),
            ct => Task.FromResult("OK"),
            CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var validator = new TestRequestValidator();
        var sut = new ValidationBehavior<TestRequest, string>(new[] { validator });

        var result = await sut.Handle(new TestRequest("valid"),
            ct => Task.FromResult("OK"),
            CancellationToken.None);

        result.Should().Be("OK");
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var validator = new TestRequestValidator();
        var sut = new ValidationBehavior<TestRequest, string>(new[] { validator });

        var act = () => sut.Handle(new TestRequest(""),
            ct => Task.FromResult("OK"),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task Handle_MultipleValidators_AggregatesErrors()
    {
        var validator1 = new TestRequestValidator();
        var validator2 = new AnotherTestRequestValidator();
        var sut = new ValidationBehavior<TestRequest, string>(new IValidator<TestRequest>[] { validator1, validator2 });

        var act = () => sut.Handle(new TestRequest(""),
            ct => Task.FromResult("OK"),
            CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().ContainKey("Name");
    }

    public record TestRequest(string Name) : IRequest<string>;

    private class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator() => RuleFor(x => x.Name).NotEmpty();
    }

    private class AnotherTestRequestValidator : AbstractValidator<TestRequest>
    {
        public AnotherTestRequestValidator() => RuleFor(x => x.Name).MinimumLength(3);
    }
}
