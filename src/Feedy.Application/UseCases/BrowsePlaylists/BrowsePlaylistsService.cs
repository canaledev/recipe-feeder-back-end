namespace Feedy.Application.UseCases.BrowsePlaylists;

using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;

public class BrowsePlaylistsService
{
    private readonly IPlaylistRepository _repository;

    public BrowsePlaylistsService(IPlaylistRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<BrowsePlaylistsResponse>> HandleAsync(
        BrowsePlaylistsRequest request, CancellationToken cancellationToken)
    {
        var (items, total) = await _repository.GetAllAsync(
            request.Search,
            request.DietaryRegime,
            request.Difficulty,
            request.Sort,
            request.Page,
            request.PageSize,
            userId: null,
            cancellationToken);

        var dtos = items.Select(p => new PlaylistSummaryDto(
            p.Id, p.Title, p.Description, p.CoverImageUrl,
            p.Author, p.RecipeCount, p.AvgPrepTimeMinutes,
            p.Difficulty, p.Rating, p.IsSubscribed));

        return Result<BrowsePlaylistsResponse>.Ok(
            new BrowsePlaylistsResponse(dtos, total, request.Page, request.PageSize));
    }
}
