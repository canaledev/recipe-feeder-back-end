namespace Feedy.Application.UseCases.SubscribeToPlaylist;

using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;

public class SubscribeToPlaylistService
{
    private static readonly Error AlreadySubscribed =
        new("ALREADY_SUBSCRIBED", "User is already subscribed to this playlist.");

    private readonly IPlaylistRepository _repository;

    public SubscribeToPlaylistService(IPlaylistRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Nothing>> HandleAsync(
        SubscribeToPlaylistRequest request, CancellationToken cancellationToken)
    {
        if (await _repository.IsSubscribedAsync(request.UserId, request.PlaylistId, cancellationToken))
            return Result<Nothing>.Fail(AlreadySubscribed);

        await _repository.SubscribeAsync(request.UserId, request.PlaylistId, cancellationToken);
        return Result<Nothing>.Ok(Nothing.Value);
    }
}
