namespace Feedy.Application.UseCases.GetRecipeFeed;

/// <summary>
/// Smart Feed ranking engine. Scores recipes against user profile.
/// Use case specific service. NOT shared with other use cases.
/// </summary>
internal class FeedRankingService
{
    /// <summary>
    /// Score a recipe against user preferences.
    /// Returns 0-100 match percentage.
    /// </summary>
    public int RankRecipe(
        string[] recipeTags,
        string recipeDifficulty,
        string[] userFlavorTags,
        string[] userDietaryRegime,
        string[] userRejectedIngredients)
    {
        // Start with base score
        int score = 50;

        // Boost by matching flavor tags
        var matchingTags = recipeTags.Intersect(userFlavorTags).Count();
        score += matchingTags * 10;

        // Penalty for rejected ingredients (assume empty for now, implement when ingredient data is available)
        // var rejectedCount = recipe.Ingredients.Intersect(userRejectedIngredients).Count();
        // score -= rejectedCount * 20;

        // Ensure score stays in 0-100 range
        return Math.Max(0, Math.Min(100, score));
    }
}
