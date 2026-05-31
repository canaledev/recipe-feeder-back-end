namespace Feedy.Application.UseCases.GetPlaylistDetail;

/// <summary>
/// Scores a playlist recipe item against the user's profile.
/// Intentionally duplicates FeedRankingService — use-case specific per vertical slice rule.
/// Returns 0–100 match percentage.
/// </summary>
internal class PlaylistItemRankingService
{
    public int RankItem(string[] itemTags, string[] userFlavorTags)
    {
        int score = 50;
        var matchingTags = itemTags.Intersect(userFlavorTags).Count();
        score += matchingTags * 10;
        return Math.Max(0, Math.Min(100, score));
    }
}
