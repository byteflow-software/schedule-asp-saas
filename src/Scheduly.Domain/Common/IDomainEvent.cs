namespace Scheduly.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
