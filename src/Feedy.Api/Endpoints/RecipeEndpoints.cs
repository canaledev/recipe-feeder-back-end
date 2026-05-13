namespace Feedy.Api.Endpoints;

using Feedy.Application.UseCases.GetRecipeFeed;

/// <summary>
/// Recipe-related endpoints. Feed and detail routes live here.
/// </summary>
public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/recipes")
            .WithName("Recipes");

        group.MapGet("/feed", GetFeed)
            .WithName("GetFeed")
            .WithOpenApi()
            .RequireAuthorization();
    }

    private static async Task<IResult> GetFeed(
        GetRecipeFeedQueryHandler handler,
        CancellationToken cancellationToken = default)
    {
        // TODO: extract UserId from auth claims
        var query = new GetRecipeFeedQuery(Guid.Empty);
        var result = await handler.HandleAsync(query, cancellationToken);
        return Results.Ok(result);
    }
}
