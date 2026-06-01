namespace Feedy.Application.UseCases.GetSubscribedPlaylists;

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
