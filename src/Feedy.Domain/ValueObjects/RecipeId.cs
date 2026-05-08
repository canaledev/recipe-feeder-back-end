namespace Feedy.Domain.ValueObjects;

/// <summary>
/// RecipeId value object. Strongly-typed identifier for Recipe aggregate.
/// </summary>
public record RecipeId(Guid Value)
{
    public static RecipeId NewId() => new(Guid.NewGuid());
    public static RecipeId From(Guid guid) => new(guid);

    public override string ToString() => Value.ToString();
}
