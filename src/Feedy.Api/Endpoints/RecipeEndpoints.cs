namespace Feedy.Api.Endpoints;

using Feedy.Application.UseCases.GetRecipeFeed;

/// <summary>
/// Recipe-related endpoints.
/// </summary>
public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api")
            .WithName("Recipes");

        group.MapGet("/feed", GetFeed)
            .WithName("GetFeed")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<GetRecipeFeedResult>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetFeed(
        Guid userId,
        GetRecipeFeedQueryHandler handler,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query  = new GetRecipeFeedQuery(userId, page, pageSize);
        var result = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(result);
    }
}
