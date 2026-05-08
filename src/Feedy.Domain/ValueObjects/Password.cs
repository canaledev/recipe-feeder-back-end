namespace Feedy.Domain.ValueObjects;

/// <summary>
/// Password value object. Immutable, stores hashed value.
/// Encapsulates password validation rules.
/// </summary>
public record Password
{
    public string HashedValue { get; }

    private Password(string hashedValue)
    {
        HashedValue = hashedValue;
    }

    public static Password Create(string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
            throw new ArgumentException("Password cannot be empty", nameof(rawPassword));

        if (rawPassword.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters", nameof(rawPassword));

        // TODO: Implement bcrypt hashing via PasswordHasher (domain service)
        var hashed = $"hashed_{rawPassword}";

        return new Password(hashed);
    }

    public override string ToString() => "[REDACTED]";
}
