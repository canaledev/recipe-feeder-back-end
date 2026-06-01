namespace Feedy.Domain.Entities;

/// <summary>
/// Data contract for a playlist summary returned by the repository.
/// Used by browse and subscribed-list use cases.
/// </summary>
public record PlaylistData(
    Guid Id,
    string Title,
    string Description,
    string CoverImageUrl,
    string Author,
    int RecipeCount,
    double AvgPrepTimeMinutes,
    string Difficulty,
    double Rating,
    bool IsSubscribed,
    DateTime CreatedAt
);

/// <summary>
/// Data contract for a single item inside a playlist detail view.
/// Includes recipe fields needed to compute matchPercentage in the application layer.
/// </summary>
public record PlaylistItemData(
    Guid RecipeId,
    string Title,
    string ThumbnailUrl,
    int PrepTimeMinutes,
    string Difficulty,
    string[] Tags,
    string CompletionState
);
