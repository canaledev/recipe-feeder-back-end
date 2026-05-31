namespace Feedy.Application.UseCases.GetPlaylistDetail;

using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;

public class GetPlaylistDetailService
{
    private static readonly Error NotFound = new("PLAYLIST_NOT_FOUND", "Playlist not found.");

    private readonly IPlaylistRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly PlaylistItemRankingService _ranking = new();

    public GetPlaylistDetailService(IPlaylistRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<Result<GetPlaylistDetailResponse>> HandleAsync(
        GetPlaylistDetailRequest request, CancellationToken cancellationToken)
    {
        var playlist = await _repository.GetByIdAsync(request.PlaylistId, request.UserId, cancellationToken);
        if (playlist is null)
            return Result<GetPlaylistDetailResponse>.Fail(NotFound);

        var rawItems = (await _repository.GetItemsAsync(request.PlaylistId, request.UserId, cancellationToken))
            .ToList();

        var profile = await _userRepository.GetProfileAsync(request.UserId, cancellationToken);
        var userFlavorTags = profile?.FlavorTags ?? [];

        var items = rawItems.Select(i => new PlaylistItemDto(
            i.RecipeId,
            i.Title,
            i.ThumbnailUrl,
            i.PrepTimeMinutes,
            i.Difficulty,
            _ranking.RankItem(i.Tags, userFlavorTags),
            i.CompletionState));

        // Playhead: index of the first item that has not yet been completed in a prior pass.
        var completedStates = new HashSet<string> { "previous_done", "historical_done", "skipped" };
        var playheadIndex = rawItems.FindIndex(i => !completedStates.Contains(i.CompletionState));
        if (playheadIndex < 0) playheadIndex = 0;

        return Result<GetPlaylistDetailResponse>.Ok(new GetPlaylistDetailResponse(
            playlist.Id,
            playlist.Title,
            playlist.Description,
            playlist.CoverImageUrl,
            playlist.Author,
            playlist.RecipeCount,
            playlist.AvgPrepTimeMinutes,
            playlist.Difficulty,
            playlist.Rating,
            playheadIndex,
            items));
    }
}
