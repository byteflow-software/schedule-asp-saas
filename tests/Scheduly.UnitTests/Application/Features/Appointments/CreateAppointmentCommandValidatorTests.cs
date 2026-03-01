using FluentAssertions;
using Scheduly.Application.Features.Appointments.Commands.CreateAppointment;

namespace Scheduly.UnitTests.Application.Features.Appointments;

public class CreateAppointmentCommandValidatorTests
{
    private readonly CreateAppointmentCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_HasNoErrors()
    {
        var command = new CreateAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            "Some notes");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyCustomerId_HasError()
    {
        var command = new CreateAppointmentCommand(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public async Task Validate_EmptyServiceId_HasError()
    {
        var command = new CreateAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            null,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ServiceId");
    }

    [Fact]
    public async Task Validate_StartTimeInPast_HasError()
    {
        var command = new CreateAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddHours(1),
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StartTime");
    }

    [Fact]
    public async Task Validate_EndTimeBeforeStartTime_HasError()
    {
        var startTime = DateTime.UtcNow.AddDays(1);
        var command = new CreateAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            startTime,
            startTime.AddHours(-1),
            null);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndTime");
    }
}
