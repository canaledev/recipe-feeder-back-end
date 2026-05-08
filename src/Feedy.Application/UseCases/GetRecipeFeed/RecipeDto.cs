namespace Feedy.Application.UseCases.GetRecipeFeed;

/// <summary>
/// DTO for recipe in the Smart Feed. Use case specific.
/// This shape is calculated by FeedRankingService (matchPercentage).
/// NOT shared with other use cases.
/// </summary>
public class RecipeDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int MatchPercentage { get; init; }
    public string[] Tags { get; init; } = [];
    public string Difficulty { get; init; } = string.Empty;
    public int PrepTimeMinutes { get; init; }
    public bool IsSubscribedSequence { get; init; }
    public string MainImageUrl { get; init; } = string.Empty;
}
