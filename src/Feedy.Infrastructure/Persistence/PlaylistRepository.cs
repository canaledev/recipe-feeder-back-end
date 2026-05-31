namespace Feedy.Infrastructure.Persistence;

using Dapper;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using Npgsql;

/// <summary>
/// Playlist repository implementation using Dapper and PostgreSQL.
/// </summary>
internal class PlaylistRepository : IPlaylistRepository
{
    private readonly string _connectionString;

    public PlaylistRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<(IEnumerable<PlaylistData> Items, int Total)> GetAllAsync(
        string? search,
        string? dietaryRegime,
        string? difficulty,
        string sort,
        int page,
        int pageSize,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var orderClause = sort switch
        {
            "popularity" => "p.rating DESC",
            "newest"     => "p.created_at DESC",
            _            => "p.created_at DESC"
        };

        const string baseWhere = @"
            WHERE (@search IS NULL OR p.title ILIKE '%' || @search || '%')
              AND (@difficulty IS NULL OR p.difficulty = @difficulty)";

        var countSql = $@"
            SELECT COUNT(*) FROM playlists p
            {baseWhere}";

        var dataSql = $@"
            SELECT
                p.id,
                p.title,
                p.description,
                p.cover_image_url       AS CoverImageUrl,
                p.author,
                COUNT(pi.id)::INT            AS RecipeCount,
                CAST(0.0 AS FLOAT8)          AS AvgPrepTimeMinutes,
                p.difficulty,
                CAST(p.rating AS FLOAT8)     AS Rating,
                CASE WHEN ups.id IS NOT NULL THEN TRUE ELSE FALSE END AS IsSubscribed,
                p.created_at                 AS CreatedAt
            FROM playlists p
            LEFT JOIN playlist_items pi ON pi.playlist_id = p.id
            LEFT JOIN user_playlist_subscriptions ups
                ON ups.playlist_id = p.id
               AND ups.user_id = @userId
               AND ups.unsubscribed_at IS NULL
            {baseWhere}
            GROUP BY p.id, ups.id
            ORDER BY {orderClause}
            LIMIT @pageSize OFFSET @offset";

        var parameters = new
        {
            search,
            difficulty,
            userId = userId ?? Guid.Empty,
            pageSize,
            offset = (page - 1) * pageSize
        };

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var total = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<PlaylistData>(dataSql, parameters);
        return (items, total);
    }

    public async Task<IEnumerable<PlaylistData>> GetSubscribedByUserAsync(
        Guid userId,
        string? dietaryRegime,
        string? difficulty,
        string? completionStatus,
        string sort,
        CancellationToken cancellationToken)
    {
        var orderClause = sort switch
        {
            "alphabetical"     => "p.title ASC",
            "matchPercentage"  => "p.rating DESC",
            _                  => "ups.subscribed_at DESC"
        };

        var sql = $@"
            SELECT
                p.id,
                p.title,
                p.description,
                p.cover_image_url   AS CoverImageUrl,
                p.author,
                COUNT(pi.id)::INT            AS RecipeCount,
                CAST(0.0 AS FLOAT8)          AS AvgPrepTimeMinutes,
                p.difficulty,
                CAST(p.rating AS FLOAT8)     AS Rating,
                TRUE                         AS IsSubscribed,
                p.created_at                 AS CreatedAt
            FROM playlists p
            INNER JOIN user_playlist_subscriptions ups
                ON ups.playlist_id = p.id
               AND ups.user_id = @userId
               AND ups.unsubscribed_at IS NULL
            LEFT JOIN playlist_items pi ON pi.playlist_id = p.id
            WHERE (@difficulty IS NULL OR p.difficulty = @difficulty)
            GROUP BY p.id, ups.subscribed_at
            ORDER BY {orderClause}";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.QueryAsync<PlaylistData>(sql, new { userId, difficulty });
    }

    public async Task<PlaylistData?> GetByIdAsync(
        Guid playlistId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                p.id,
                p.title,
                p.description,
                p.cover_image_url   AS CoverImageUrl,
                p.author,
                COUNT(pi.id)::INT            AS RecipeCount,
                CAST(0.0 AS FLOAT8)          AS AvgPrepTimeMinutes,
                p.difficulty,
                CAST(p.rating AS FLOAT8)     AS Rating,
                CASE WHEN ups.id IS NOT NULL THEN TRUE ELSE FALSE END AS IsSubscribed,
                p.created_at                 AS CreatedAt
            FROM playlists p
            LEFT JOIN playlist_items pi ON pi.playlist_id = p.id
            LEFT JOIN user_playlist_subscriptions ups
                ON ups.playlist_id = p.id
               AND ups.user_id = @userId
               AND ups.unsubscribed_at IS NULL
            WHERE p.id = @playlistId
            GROUP BY p.id, ups.id";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<PlaylistData>(sql, new { playlistId, userId });
    }

    public async Task<IEnumerable<PlaylistItemData>> GetItemsAsync(
        Guid playlistId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Tags are not stored on playlist_items — falling back to empty array until recipes table is joined.
        // completion_state comes from user_playlist_items; defaults to 'never_done' when no row exists.
        const string sql = @"
            SELECT
                pi.recipe_id            AS RecipeId,
                pi.recipe_id::text      AS Title,
                ''                      AS ThumbnailUrl,
                0                       AS PrepTimeMinutes,
                'Easy'                  AS Difficulty,
                ARRAY[]::TEXT[]         AS Tags,
                COALESCE(upi.completion_state, 'never_done') AS CompletionState
            FROM playlist_items pi
            LEFT JOIN user_playlist_items upi
                ON upi.playlist_id = pi.playlist_id
               AND upi.recipe_id   = pi.recipe_id
               AND upi.user_id     = @userId
            WHERE pi.playlist_id = @playlistId
            ORDER BY pi.ordinal_index";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.QueryAsync<PlaylistItemData>(sql, new { playlistId, userId });
    }

    public async Task<bool> IsSubscribedAsync(
        Guid userId,
        Guid playlistId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT COUNT(1) FROM user_playlist_subscriptions
            WHERE user_id = @userId
              AND playlist_id = @playlistId
              AND unsubscribed_at IS NULL";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<int>(sql, new { userId, playlistId }) > 0;
    }

    public async Task SubscribeAsync(
        Guid userId,
        Guid playlistId,
        CancellationToken cancellationToken)
    {
        // Insert subscription + seed completion rows in a single transaction.
        const string insertSubscription = @"
            INSERT INTO user_playlist_subscriptions (user_id, playlist_id)
            VALUES (@userId, @playlistId)
            ON CONFLICT (user_id, playlist_id) DO UPDATE SET unsubscribed_at = NULL";

        const string seedItems = @"
            INSERT INTO user_playlist_items (user_id, playlist_id, recipe_id, completion_state)
            SELECT @userId, @playlistId, pi.recipe_id, 'never_done'
            FROM playlist_items pi
            WHERE pi.playlist_id = @playlistId
            ON CONFLICT (user_id, playlist_id, recipe_id) DO NOTHING";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(insertSubscription, new { userId, playlistId }, transaction);
        await connection.ExecuteAsync(seedItems, new { userId, playlistId }, transaction);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task UnsubscribeAsync(
        Guid userId,
        Guid playlistId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE user_playlist_subscriptions
            SET unsubscribed_at = NOW()
            WHERE user_id = @userId
              AND playlist_id = @playlistId
              AND unsubscribed_at IS NULL";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await connection.ExecuteAsync(sql, new { userId, playlistId });
    }
}
