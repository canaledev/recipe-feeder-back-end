namespace Feedy.Application.UseCases.GetSubscribedPlaylists;

using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;

public class GetSubscribedPlaylistsService
{
    private readonly IPlaylistRepository _repository;

    public GetSubscribedPlaylistsService(IPlaylistRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GetSubscribedPlaylistsResponse>> HandleAsync(
        GetSubscribedPlaylistsRequest request, CancellationToken cancellationToken)
    {
        var playlists = await _repository.GetSubscribedByUserAsync(
            request.UserId,
            request.DietaryRegime,
            request.Difficulty,
            request.CompletionStatus,
            request.Sort,
            cancellationToken);

        var dtos = playlists.Select(p => new PlaylistSummaryDto(
            p.Id, p.Title, p.Description, p.CoverImageUrl,
            p.Author, p.RecipeCount, p.AvgPrepTimeMinutes,
            p.Difficulty, p.Rating, p.IsSubscribed));

        return Result<GetSubscribedPlaylistsResponse>.Ok(new GetSubscribedPlaylistsResponse(dtos));
    }
}
