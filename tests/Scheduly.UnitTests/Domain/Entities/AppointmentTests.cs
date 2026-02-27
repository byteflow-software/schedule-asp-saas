using FluentAssertions;
using Scheduly.Domain.Entities;

namespace Scheduly.UnitTests.Domain.Entities;

public class AppointmentTests
{
    [Fact]
    public void OverlapsWith_WhenTimesOverlap_ReturnsTrue()
    {
        var appointment = new Appointment
        {
            StartTime = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc)
        };

        var result = appointment.OverlapsWith(
            new DateTime(2026, 3, 1, 10, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 1, 11, 30, 0, DateTimeKind.Utc));

        result.Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_WhenTimesDoNotOverlap_ReturnsFalse()
    {
        var appointment = new Appointment
        {
            StartTime = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc)
        };

        var result = appointment.OverlapsWith(
            new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));

        result.Should().BeFalse();
    }

    [Fact]
    public void OverlapsWith_WhenNewAppointmentContainsExisting_ReturnsTrue()
    {
        var appointment = new Appointment
        {
            StartTime = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc)
        };

        var result = appointment.OverlapsWith(
            new DateTime(2026, 3, 1, 9, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));

        result.Should().BeTrue();
    }

    [Fact]
    public void OverlapsWith_WhenExactlyAdjacentAfter_ReturnsFalse()
    {
        var appointment = new Appointment
        {
            StartTime = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc)
        };

        var result = appointment.OverlapsWith(
            new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));

        result.Should().BeFalse();
    }
}
