namespace Feedy.Application.UseCases.GetRecipeFeed;

using Feedy.Application.Interfaces;
using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;

/// <summary>
/// Orchestrates the GetRecipeFeed use case: loads user profile, ranks recipes, paginates.
/// </summary>
public class GetRecipeFeedService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IUserRepository _userRepository;
    private readonly FeedRankingService _rankingService;
    private readonly ITranslationService _translationService;

    public GetRecipeFeedService(
        IRecipeRepository recipeRepository,
        IUserRepository userRepository,
        FeedRankingService rankingService,
        ITranslationService translationService)
    {
        _recipeRepository    = recipeRepository;
        _userRepository      = userRepository;
        _rankingService      = rankingService;
        _translationService  = translationService;
    }

    public async Task<Result<GetRecipeFeedResponse>> HandleAsync(
        GetRecipeFeedRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement full logic
        // 1. Load user profile from _userRepository
        // 2. Load candidate recipes from _recipeRepository
        // 3. Rank recipes using _rankingService
        // 4. For each recipe, translate title/description/tags via _translationService.GetContentAsync
        //    using the language from the current request culture (CultureInfo.CurrentUICulture.Name)
        // 5. Map to RecipeDto (use-case specific)
        // 6. Apply pagination
        var response = new GetRecipeFeedResponse([], 0, request.PageNumber, request.PageSize);
        return await Task.FromResult(Result<GetRecipeFeedResponse>.Ok(response));
    }
}
