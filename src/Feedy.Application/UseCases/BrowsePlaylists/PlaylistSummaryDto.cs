namespace Feedy.Application.UseCases.BrowsePlaylists;

public record PlaylistSummaryDto(
    Guid Id,
    string Title,
    string Description,
    string CoverImageUrl,
    string Author,
    int RecipeCount,
    double AvgPrepTimeMinutes,
    string Difficulty,
    double Rating,
    bool IsSubscribed
);
