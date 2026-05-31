namespace Feedy.Application.UseCases.UnsubscribeFromPlaylist;

public record UnsubscribeFromPlaylistRequest(Guid UserId, Guid PlaylistId);
