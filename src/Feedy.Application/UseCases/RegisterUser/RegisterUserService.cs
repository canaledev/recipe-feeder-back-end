namespace Feedy.Application.UseCases.RegisterUser;

using Feedy.Application.Configuration;
using Feedy.Application.Helpers;
using Feedy.Application.Interfaces;
using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;
using Microsoft.Extensions.Options;

/// <summary>
/// Orchestrates the RegisterUser use case.
/// Returns a Result — no exceptions for business rule violations.
/// </summary>
public class RegisterUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly int _refreshTokenExpiryDays;

    public RegisterUserService(
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

    public async Task<Result<RegisterUserResponse>> HandleAsync(
        RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            return Result<RegisterUserResponse>.Fail(
                new Error("DUPLICATE_EMAIL", $"A user with email '{request.Email}' already exists."));

        var passwordHash = _passwordHasher.Hash(request.Password);

        var userId = await _userRepository.CreateAsync(
            request.Email,
            passwordHash,
            request.FullName,
            cancellationToken);

        var accessToken = _jwtTokenProvider.GenerateAccessToken(userId, request.Email, request.FullName);
        var (rawRefreshToken, tokenHash) = RefreshTokenHelper.GenerateTokenPair();
        var expiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        await _refreshTokenRepository.StoreAsync(userId, tokenHash, expiresAt, cancellationToken);

        return Result<RegisterUserResponse>.Ok(
            new RegisterUserResponse(accessToken, rawRefreshToken, expiresAt, userId, request.Email, request.FullName));
    }
}
