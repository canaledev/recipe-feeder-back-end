namespace Feedy.Api.Controllers;

using System.Security.Claims;
using Feedy.Application.UseCases.BrowsePlaylists;
using Feedy.Application.UseCases.GetPlaylistDetail;
using Feedy.Application.UseCases.GetSubscribedPlaylists;
using Feedy.Application.UseCases.SubscribeToPlaylist;
using Feedy.Application.UseCases.UnsubscribeFromPlaylist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/playlists")]
public class PlaylistsController : ControllerBase
{
    private readonly BrowsePlaylistsService _browseService;
    private readonly GetSubscribedPlaylistsService _subscribedService;
    private readonly GetPlaylistDetailService _detailService;
    private readonly SubscribeToPlaylistService _subscribeService;
    private readonly UnsubscribeFromPlaylistService _unsubscribeService;

    public PlaylistsController(
        BrowsePlaylistsService browseService,
        GetSubscribedPlaylistsService subscribedService,
        GetPlaylistDetailService detailService,
        SubscribeToPlaylistService subscribeService,
        UnsubscribeFromPlaylistService unsubscribeService)
    {
        _browseService = browseService;
        _subscribedService = subscribedService;
        _detailService = detailService;
        _subscribeService = subscribeService;
        _unsubscribeService = unsubscribeService;
    }

    /// <summary>Browse all available playlists (paginated, public).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<BrowsePlaylistsResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Browse(
        [FromQuery] string? search = null,
        [FromQuery] string? dietaryRegime = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string sort = "relevance",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var request = new BrowsePlaylistsRequest(search, dietaryRegime, difficulty, sort, page, pageSize);
        var result = await _browseService.HandleAsync(request, cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>List playlists the authenticated user is subscribed to.</summary>
    [HttpGet("subscribed")]
    [Authorize]
    [ProducesResponseType<GetSubscribedPlaylistsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubscribed(
        [FromQuery] string? dietaryRegime = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? completionStatus = null,
        [FromQuery] string sort = "subscriptionDate",
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var request = new GetSubscribedPlaylistsRequest(userId, dietaryRegime, difficulty, completionStatus, sort);
        var result = await _subscribedService.HandleAsync(request, cancellationToken);
        return Ok(result.Value);
    }

    /// <summary>Full detail for a single playlist including its recipe list.</summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType<GetPlaylistDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetail(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _detailService.HandleAsync(new GetPlaylistDetailRequest(id, userId), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { result.Error.Code, result.Error.Message });
    }

    /// <summary>Subscribe the authenticated user to a playlist.</summary>
    [HttpPost("{id:guid}/subscription")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Subscribe(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _subscribeService.HandleAsync(new SubscribeToPlaylistRequest(userId, id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : Conflict(new { result.Error.Code, result.Error.Message });
    }

    /// <summary>Unsubscribe the authenticated user from a playlist.</summary>
    [HttpDelete("{id:guid}/subscription")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unsubscribe(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _unsubscribeService.HandleAsync(new UnsubscribeFromPlaylistRequest(userId, id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : NotFound(new { result.Error.Code, result.Error.Message });
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
