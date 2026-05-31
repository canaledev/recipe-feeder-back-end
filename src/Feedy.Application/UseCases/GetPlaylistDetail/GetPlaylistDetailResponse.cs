namespace Feedy.Application.UseCases.GetPlaylistDetail;

public record GetPlaylistDetailResponse(
    Guid Id,
    string Title,
    string Description,
    string CoverImageUrl,
    string Author,
    int RecipeCount,
    double AvgPrepTimeMinutes,
    string Difficulty,
    double Rating,
    int CurrentPlayheadIndex,
    IEnumerable<PlaylistItemDto> Items
);
