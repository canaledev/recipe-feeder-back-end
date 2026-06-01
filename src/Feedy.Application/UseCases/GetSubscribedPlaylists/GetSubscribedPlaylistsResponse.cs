namespace Feedy.Application.UseCases.GetSubscribedPlaylists;

public record GetSubscribedPlaylistsResponse(IEnumerable<PlaylistSummaryDto> Items);
