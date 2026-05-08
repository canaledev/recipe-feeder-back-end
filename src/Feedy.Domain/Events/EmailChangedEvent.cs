namespace Feedy.Domain.Events;

using Feedy.Domain.ValueObjects;

/// <summary>
/// Domain event: User changed their email.
/// Published when User aggregate changes email via ChangeEmail().
/// Handlers: verify new email, update external systems.
/// </summary>
public record EmailChangedEvent(UserId UserId, string OldEmail, string NewEmail, DateTime ChangedAt) : DomainEvent;
