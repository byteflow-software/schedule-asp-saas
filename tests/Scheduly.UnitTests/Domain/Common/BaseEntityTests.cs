using FluentAssertions;
using Scheduly.Domain.Common;

namespace Scheduly.UnitTests.Domain.Common;

public class BaseEntityTests
{
    [Fact]
    public void AddDomainEvent_AddsToCollection()
    {
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        entity.AddDomainEvent(domainEvent);

        entity.DomainEvents.Should().ContainSingle().Which.Should().Be(domainEvent);
    }

    [Fact]
    public void RemoveDomainEvent_RemovesFromCollection()
    {
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);

        entity.RemoveDomainEvent(domainEvent);

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_RemovesAll()
    {
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestDomainEvent());
        entity.AddDomainEvent(new TestDomainEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ReturnsReadOnlyCollection()
    {
        var entity = new TestEntity();
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Id_CanBeSetAndGet()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id };
        entity.Id.Should().Be(id);
    }

    private class TestEntity : BaseEntity { }

    private class TestDomainEvent : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
