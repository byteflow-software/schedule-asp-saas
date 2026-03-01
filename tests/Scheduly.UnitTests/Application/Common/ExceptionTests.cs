using FluentAssertions;
using FluentValidation.Results;
using Scheduly.Application.Common.Exceptions;
using ValidationException = Scheduly.Application.Common.Exceptions.ValidationException;

namespace Scheduly.UnitTests.Application.Common;

public class ExceptionTests
{
    [Fact]
    public void ForbiddenAccessException_HasDefaultMessage()
    {
        var ex = new ForbiddenAccessException();
        ex.Message.Should().Be("You do not have permission to perform this action.");
    }

    [Fact]
    public void ValidationException_DefaultConstructor_HasEmptyErrors()
    {
        var ex = new ValidationException();
        ex.Message.Should().Be("One or more validation failures have occurred.");
        ex.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationException_WithFailures_GroupsByPropertyName()
    {
        var failures = new List<ValidationFailure>
        {
            new("Email", "Email is required."),
            new("Email", "Email must be valid."),
            new("Password", "Password is required.")
        };

        var ex = new ValidationException(failures);

        ex.Errors.Should().HaveCount(2);
        ex.Errors["Email"].Should().HaveCount(2);
        ex.Errors["Password"].Should().HaveCount(1);
    }
}
