using FluentAssertions;
using Scheduly.Application.Features.Services.Commands.CreateService;

namespace Scheduly.UnitTests.Application.Features.Services;

public class CreateServiceCommandValidatorTests
{
    private readonly CreateServiceCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_HasNoErrors()
    {
        var command = new CreateServiceCommand("Check-up", "Regular check-up", 60, 5000);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyName_HasError()
    {
        var command = new CreateServiceCommand("", "Description", 60, 5000);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_ZeroDuration_HasError()
    {
        var command = new CreateServiceCommand("Check-up", null, 0, 5000);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DurationMinutes");
    }

    [Fact]
    public async Task Validate_NegativePrice_HasError()
    {
        var command = new CreateServiceCommand("Check-up", null, 60, -100);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PriceInCents");
    }
}
