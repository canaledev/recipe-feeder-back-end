namespace Feedy.Application.UseCases.GetRecipeFeed;

/// <summary>
/// Paginated response for the GetRecipeFeed use case.
/// </summary>
public record GetRecipeFeedResponse(
    IEnumerable<RecipeDto> Recipes,
    int TotalCount,
    int PageNumber,
    int PageSize);
