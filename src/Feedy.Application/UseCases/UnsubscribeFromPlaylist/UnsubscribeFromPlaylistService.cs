namespace Feedy.Application.UseCases.UnsubscribeFromPlaylist;

using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;

public class UnsubscribeFromPlaylistService
{
    private static readonly Error NotSubscribed =
        new("NOT_SUBSCRIBED", "User is not subscribed to this playlist.");

    private readonly IPlaylistRepository _repository;

    public UnsubscribeFromPlaylistService(IPlaylistRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Nothing>> HandleAsync(
        UnsubscribeFromPlaylistRequest request, CancellationToken cancellationToken)
    {
        if (!await _repository.IsSubscribedAsync(request.UserId, request.PlaylistId, cancellationToken))
            return Result<Nothing>.Fail(NotSubscribed);

        await _repository.UnsubscribeAsync(request.UserId, request.PlaylistId, cancellationToken);
        return Result<Nothing>.Ok(Nothing.Value);
    }
}
