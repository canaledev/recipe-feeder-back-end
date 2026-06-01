namespace Feedy.Domain.Interfaces;

using Feedy.Domain.Entities;

/// <summary>
/// Repository for playlist catalog and user subscription data.
/// </summary>
public interface IPlaylistRepository
{
    /// <summary>
    /// Paginated browse of all playlists. Optional userId populates IsSubscribed.
    /// </summary>
    Task<(IEnumerable<PlaylistData> Items, int Total)> GetAllAsync(
        string? search,
        string? dietaryRegime,
        string? difficulty,
        string sort,
        int page,
        int pageSize,
        Guid? userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// All playlists the user is actively subscribed to (unsubscribed_at IS NULL).
    /// </summary>
    Task<IEnumerable<PlaylistData>> GetSubscribedByUserAsync(
        Guid userId,
        string? dietaryRegime,
        string? difficulty,
        string? completionStatus,
        string sort,
        CancellationToken cancellationToken);

    /// <summary>
    /// Full playlist header. Returns null if not found.
    /// </summary>
    Task<PlaylistData?> GetByIdAsync(
        Guid playlistId,
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Ordered items for a playlist, including the user's completion state for each recipe.
    /// </summary>
    Task<IEnumerable<PlaylistItemData>> GetItemsAsync(
        Guid playlistId,
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// True when an active subscription exists (unsubscribed_at IS NULL).
    /// </summary>
    Task<bool> IsSubscribedAsync(
        Guid userId,
        Guid playlistId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Inserts a subscription row and seeds user_playlist_items rows with 'never_done'.
    /// Both writes are performed in a single transaction.
    /// </summary>
    Task SubscribeAsync(
        Guid userId,
        Guid playlistId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Soft-deletes the subscription by setting unsubscribed_at = NOW().
    /// Completion history in user_playlist_items is preserved.
    /// </summary>
    Task UnsubscribeAsync(
        Guid userId,
        Guid playlistId,
        CancellationToken cancellationToken);
}
