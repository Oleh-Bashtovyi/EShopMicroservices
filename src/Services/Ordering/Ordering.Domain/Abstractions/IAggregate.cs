namespace Ordering.Domain.Abstractions;

public interface IAggregate<T> : IAggregate, IEntity<T>
{

}


public interface IAggregate : IEntity
{
    public IReadOnlyList<IDomainEvent> DomainEvents { get; }
    public IDomainEvent[] ClearDomainEvents();
}