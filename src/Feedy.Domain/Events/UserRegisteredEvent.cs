namespace Feedy.Domain.Events;

using Feedy.Domain.ValueObjects;

/// <summary>
/// Domain event: User registered successfully.
/// Published when User aggregate creates a new user.
/// Handlers: send welcome email, initialize profile, log signup metrics.
/// </summary>
public record UserRegisteredEvent(UserId UserId, string Email, DateTime RegisteredAt) : DomainEvent;
