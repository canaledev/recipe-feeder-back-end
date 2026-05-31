namespace Feedy.Application.UseCases.GetSubscribedPlaylists;

public record GetSubscribedPlaylistsRequest(
    Guid UserId,
    string? DietaryRegime,
    string? Difficulty,
    string? CompletionStatus,
    string Sort
);
