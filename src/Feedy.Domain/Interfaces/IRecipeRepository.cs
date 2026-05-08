namespace Feedy.Domain.Interfaces;

using Feedy.Domain.Entities;

/// <summary>
/// Repository for Recipe aggregate. Implemented by Infrastructure layer.
/// </summary>
public interface IRecipeRepository
{
    /// <summary>
    /// Get recipes filtered by tags and dietary preferences.
    /// </summary>
    Task<IEnumerable<RecipeData>> GetRecipesForFeedAsync(
        IEnumerable<string> preferredTags,
        IEnumerable<string> dietaryRegime,
        CancellationToken cancellationToken);
}
