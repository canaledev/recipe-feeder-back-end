namespace Feedy.Domain.Entities;

/// <summary>
/// Recipe domain entity. Immutable data container.
/// </summary>
public record Recipe(
    Guid Id,
    string Title,
    string Description,
    string[] Tags,
    string Difficulty,
    int PrepTimeMinutes,
    string[] Ingredients,
    string MainImageUrl,
    DateTime CreatedAt
);
