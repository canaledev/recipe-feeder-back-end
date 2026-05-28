namespace Feedy.Domain.Interfaces;

using Feedy.Domain.Entities;

/// <summary>
/// Repository for refresh token lifecycle. Implemented by Infrastructure.
/// </summary>
public interface IRefreshTokenRepository
{
    Task StoreAsync(Guid userId, string tokenHash, DateTime expiresAt, CancellationToken ct);
    Task<RefreshTokenData?> GetByHashAsync(string tokenHash, CancellationToken ct);
    Task RevokeAsync(Guid tokenId, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
}
