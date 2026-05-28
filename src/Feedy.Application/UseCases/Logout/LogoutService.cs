namespace Feedy.Application.UseCases.Logout;

using Feedy.Domain.Interfaces;

/// <summary>
/// Revokes all active refresh tokens for the authenticated user.
/// Idempotent — calling when no tokens exist is a no-op.
/// </summary>
public class LogoutService
{
    private readonly IRefreshTokenRepository _refreshRepo;

    public LogoutService(IRefreshTokenRepository refreshRepo)
    {
        _refreshRepo = refreshRepo;
    }

    public async Task HandleAsync(Guid userId, CancellationToken ct)
    {
        await _refreshRepo.RevokeAllForUserAsync(userId, ct);
    }
}
