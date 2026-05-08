namespace Feedy.Domain.ValueObjects;

/// <summary>
/// UserId value object. Strongly-typed identifier for User aggregate.
/// Prevents accidental mixing of GUIDs from different domains.
/// </summary>
public record UserId(Guid Value)
{
    public static UserId NewId() => new(Guid.NewGuid());
    public static UserId From(Guid guid) => new(guid);

    public override string ToString() => Value.ToString();
}
