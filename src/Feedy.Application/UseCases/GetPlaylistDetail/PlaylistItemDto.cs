namespace Feedy.Application.UseCases.GetPlaylistDetail;

public record PlaylistItemDto(
    Guid RecipeId,
    string Title,
    string ThumbnailUrl,
    int PrepTimeMinutes,
    string Difficulty,
    int MatchPercentage,
    string CompletionState
);
