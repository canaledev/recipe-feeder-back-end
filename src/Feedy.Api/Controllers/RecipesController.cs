namespace Feedy.Api.Controllers;

using Feedy.Application.UseCases.GetRecipeFeed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/recipes")]
[Authorize]
public class RecipesController : ControllerBase
{
    private readonly GetRecipeFeedService _feedService;

    public RecipesController(GetRecipeFeedService feedService)
    {
        _feedService = feedService;
    }

    /// <summary>Get the personalized recipe feed for the authenticated user.</summary>
    [HttpGet("feed")]
    [ProducesResponseType<GetRecipeFeedResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFeed(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // TODO: extract UserId from JWT claims
        var request = new GetRecipeFeedRequest(Guid.Empty, pageNumber, pageSize);
        var result = await _feedService.HandleAsync(request, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(StatusCodes.Status500InternalServerError, new { result.Error.Code, result.Error.Message });
    }
}
