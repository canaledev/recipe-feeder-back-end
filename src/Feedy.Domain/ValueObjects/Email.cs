namespace Feedy.Domain.ValueObjects;

/// <summary>
/// Email value object. Immutable, compared by value.
/// Encapsulates email validation logic.
/// </summary>
public record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));

        if (!value.Contains("@"))
            throw new ArgumentException("Email must contain @", nameof(value));

        return new Email(value.ToLowerInvariant());
    }

    public override string ToString() => Value;
}
