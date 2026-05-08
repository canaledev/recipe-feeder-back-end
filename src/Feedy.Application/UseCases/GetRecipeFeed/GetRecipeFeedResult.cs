namespace Feedy.Application.UseCases.GetRecipeFeed;

/// <summary>
/// Result of the GetRecipeFeed query. Use case specific.
/// </summary>
public class GetRecipeFeedResult
{
    public IEnumerable<RecipeDto> Recipes { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }

    public GetRecipeFeedResult(IEnumerable<RecipeDto> recipes, int totalCount, int pageNumber, int pageSize)
    {
        Recipes = recipes;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
