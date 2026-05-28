namespace Feedy.Application.UseCases.RefreshToken;

using Feedy.Application.Configuration;
using Feedy.Application.Helpers;
using Feedy.Application.Interfaces;
using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;
using Microsoft.Extensions.Options;

/// <summary>
/// Validates an inbound refresh token (from HttpOnly cookie), rotates it, and issues a new access token.
/// Implements token-family revocation: if a revoked token is reused, all tokens for that user are invalidated.
/// </summary>
public class RefreshTokenService
{
    private static readonly Error InvalidRefreshToken =
        new("INVALID_REFRESH_TOKEN", "Refresh token is invalid or expired.");

    private readonly IRefreshTokenRepository _refreshRepo;
    private readonly IUserRepository _userRepo;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly int _refreshTokenExpiryDays;

    public RefreshTokenService(
        IRefreshTokenRepository refreshRepo,
        IUserRepository userRepo,
        IJwtTokenProvider jwtTokenProvider,
        IOptions<JwtSettings> jwtSettings)
    {
        _refreshRepo = refreshRepo;
        _userRepo = userRepo;
        _jwtTokenProvider = jwtTokenProvider;
        _refreshTokenExpiryDays = jwtSettings.Value.RefreshTokenExpirationDays;
    }

    public async Task<Result<RefreshTokenResponse>> HandleAsync(
        string? rawToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return Result<RefreshTokenResponse>.Fail(InvalidRefreshToken);

        var tokenHash = RefreshTokenHelper.HashToken(rawToken);
        var stored = await _refreshRepo.GetByHashAsync(tokenHash, ct);

        if (stored is null || stored.ExpiresAt < DateTime.UtcNow)
            return Result<RefreshTokenResponse>.Fail(InvalidRefreshToken);

        // Reuse detection: already-revoked token replayed → revoke entire family immediately.
        if (stored.RevokedAt is not null)
        {
            await _refreshRepo.RevokeAllForUserAsync(stored.UserId, ct);
            return Result<RefreshTokenResponse>.Fail(InvalidRefreshToken);
        }

        await _refreshRepo.RevokeAsync(stored.Id, ct);

        var user = await _userRepo.GetByIdAsync(stored.UserId, ct);
        if (user is null)
            return Result<RefreshTokenResponse>.Fail(InvalidRefreshToken);

        var newAccessToken = _jwtTokenProvider.GenerateAccessToken(user.Id, user.Email, user.FullName);
        var (newRawToken, newHash) = RefreshTokenHelper.GenerateTokenPair();
        var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        await _refreshRepo.StoreAsync(user.Id, newHash, expiresAt, ct);

        return Result<RefreshTokenResponse>.Ok(new RefreshTokenResponse(newAccessToken, newRawToken, expiresAt));
    }
}
