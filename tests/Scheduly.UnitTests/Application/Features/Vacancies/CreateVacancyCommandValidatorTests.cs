using FluentAssertions;
using Scheduly.Application.Features.Vacancies.Commands.CreateVacancy;

namespace Scheduly.UnitTests.Application.Features.Vacancies;

public class CreateVacancyCommandValidatorTests
{
    private readonly CreateVacancyCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_HasNoErrors()
    {
        var command = new CreateVacancyCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyUserId_HasError()
    {
        var command = new CreateVacancyCommand(
            Guid.Empty, Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Validate_EmptyServiceId_HasError()
    {
        var command = new CreateVacancyCommand(
            Guid.NewGuid(), Guid.Empty,
            DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ServiceId");
    }

    [Fact]
    public void Validate_EndTimeBeforeStartTime_HasError()
    {
        var command = new CreateVacancyCommand(
            Guid.NewGuid(), Guid.NewGuid(),
            DateTime.UtcNow.AddHours(2), DateTime.UtcNow.AddHours(1));

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndTime");
    }
}
