using MediatR;

namespace Ordering.Domain.Abstractions;


// We use INotification interface from MediatR library, 
// because it allowing domain events to be dispatched through the
// mediator handlers. So we can use mediator handlers in order to
// handle these domain event using the mediator libraries
public interface IDomainEvent : INotification
{
    public Guid EventId => Guid.NewGuid();
    public DateTime OccuredOn => DateTime.Now;
    public string EventType => GetType().AssemblyQualifiedName;
}
