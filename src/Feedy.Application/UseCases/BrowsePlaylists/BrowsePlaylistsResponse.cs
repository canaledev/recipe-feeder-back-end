namespace Feedy.Application.UseCases.BrowsePlaylists;

public record BrowsePlaylistsResponse(
    IEnumerable<PlaylistSummaryDto> Items,
    int Total,
    int Page,
    int PageSize
);
