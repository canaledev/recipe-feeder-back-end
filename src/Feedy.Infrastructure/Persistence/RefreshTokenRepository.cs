namespace Feedy.Infrastructure.Persistence;

using Dapper;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using Npgsql;

/// <summary>
/// Dapper implementation of IRefreshTokenRepository against the refresh_tokens table.
/// </summary>
internal class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly string _connectionString;

    public RefreshTokenRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task StoreAsync(Guid userId, string tokenHash, DateTime expiresAt, CancellationToken ct)
    {
        const string sql = @"
            INSERT INTO refresh_tokens (id, user_id, token_hash, expires_at, created_at)
            VALUES (@Id, @UserId, @TokenHash, @ExpiresAt, @CreatedAt)";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await connection.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        });
    }

    public async Task<RefreshTokenData?> GetByHashAsync(string tokenHash, CancellationToken ct)
    {
        const string sql = @"
            SELECT id, user_id AS UserId, token_hash AS TokenHash,
                   expires_at AS ExpiresAt, revoked_at AS RevokedAt
            FROM refresh_tokens
            WHERE token_hash = @TokenHash";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        return await connection.QueryFirstOrDefaultAsync<RefreshTokenData>(sql, new { TokenHash = tokenHash });
    }

    public async Task RevokeAsync(Guid tokenId, CancellationToken ct)
    {
        const string sql = "UPDATE refresh_tokens SET revoked_at = @RevokedAt WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await connection.ExecuteAsync(sql, new { RevokedAt = DateTime.UtcNow, Id = tokenId });
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
    {
        const string sql = @"
            UPDATE refresh_tokens
            SET revoked_at = @RevokedAt
            WHERE user_id = @UserId AND revoked_at IS NULL";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await connection.ExecuteAsync(sql, new { RevokedAt = DateTime.UtcNow, UserId = userId });
    }
}
