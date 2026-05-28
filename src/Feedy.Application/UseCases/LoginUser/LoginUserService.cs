namespace Feedy.Application.UseCases.LoginUser;

using Feedy.Application.Configuration;
using Feedy.Application.Helpers;
using Feedy.Application.Interfaces;
using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;
using Microsoft.Extensions.Options;

/// <summary>
/// Authenticates a user and issues an access token + refresh token.
/// Both "email not found" and "wrong password" return the same error to prevent account enumeration.
/// </summary>
public class LoginUserService
{
    // Static instance ensures identical Code and Message regardless of failure branch.
    private static readonly Error InvalidCredentials =
        new("INVALID_CREDENTIALS", "Invalid email or password.");

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly int _refreshTokenExpiryDays;

    public LoginUserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenProvider jwtTokenProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenProvider = jwtTokenProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenExpiryDays = jwtSettings.Value.RefreshTokenExpirationDays;
    }

    public async Task<Result<LoginUserResponse>> HandleAsync(
        LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Same branch for unknown email and wrong password — anti-enumeration.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginUserResponse>.Fail(InvalidCredentials);

        var accessToken = _jwtTokenProvider.GenerateAccessToken(user.Id, user.Email, user.FullName);
        var (rawRefreshToken, tokenHash) = RefreshTokenHelper.GenerateTokenPair();
        var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        await _refreshTokenRepository.StoreAsync(user.Id, tokenHash, expiresAt, cancellationToken);

        return Result<LoginUserResponse>.Ok(
            new LoginUserResponse(accessToken, rawRefreshToken, expiresAt, user.Id, user.Email, user.FullName));
    }
}
