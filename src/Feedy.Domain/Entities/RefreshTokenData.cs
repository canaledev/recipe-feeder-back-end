namespace Feedy.Domain.Entities;

/// <summary>
/// Dapper result record for a row in the refresh_tokens table.
/// </summary>
public record RefreshTokenData(
    Guid Id,
    Guid UserId,
    string TokenHash,
    DateTime ExpiresAt,
    DateTime? RevokedAt
);
