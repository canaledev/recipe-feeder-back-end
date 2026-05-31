namespace Feedy.Application.UseCases.BrowsePlaylists;

public record BrowsePlaylistsRequest(
    string? Search,
    string? DietaryRegime,
    string? Difficulty,
    string Sort,
    int Page,
    int PageSize
);
