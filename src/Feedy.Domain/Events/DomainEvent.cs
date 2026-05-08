namespace Feedy.Domain.Events;

/// <summary>
/// Base class for all domain events.
/// Domain events record what happened in the domain and are published to handlers.
/// </summary>
public abstract record DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public int Version { get; } = 1;
}
