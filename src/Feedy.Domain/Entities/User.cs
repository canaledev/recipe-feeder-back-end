namespace Feedy.Domain.Entities;

using Feedy.Domain.Events;
using Feedy.Domain.ValueObjects;

/// <summary>
/// User Aggregate Root. Encapsulates user identity and business rules.
/// All state changes go through public methods, not direct property assignment.
/// </summary>
public class User
{
    public UserId Id { get; private set; }
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Private constructor: only aggregate can create instances
    private User(UserId id, Email email, Password password, DateTime createdAt)
    {
        Id = id;
        Email = email;
        Password = password;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Factory method to create a new user.
    /// Raises UserRegisteredEvent.
    /// </summary>
    public static User Register(Email email, Password password)
    {
        var userId = UserId.NewId();
        var user = new User(userId, email, password, DateTime.UtcNow);

        // Record domain event
        user._domainEvents.Add(new UserRegisteredEvent(userId, email.Value, DateTime.UtcNow));

        return user;
    }

    /// <summary>
    /// Change user's email. Validates new email and raises event.
    /// </summary>
    public void ChangeEmail(Email newEmail)
    {
        if (newEmail.Equals(Email))
            return; // No change needed

        var oldEmail = Email.Value;
        Email = newEmail;

        // Record domain event
        _domainEvents.Add(new EmailChangedEvent(Id, oldEmail, newEmail.Value, DateTime.UtcNow));
    }

    /// <summary>
    /// Record a login event.
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clear recorded domain events after they are published.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
