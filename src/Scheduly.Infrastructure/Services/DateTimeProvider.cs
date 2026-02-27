using Scheduly.Application.Common.Interfaces;

namespace Scheduly.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
