namespace Feedy.Application.Tests.UseCases.RefreshToken;

using Feedy.Application.Configuration;
using Feedy.Application.Interfaces;
using Feedy.Application.UseCases.RefreshToken;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class RefreshTokenServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IJwtTokenProvider> _jwt = new();
    private readonly IOptions<JwtSettings> _settings =
        Options.Create(new JwtSettings { AccessTokenExpirationMinutes = 15, RefreshTokenExpirationDays = 30 });

    private RefreshTokenService CreateService() =>
        new(_refreshRepo.Object, _userRepo.Object, _jwt.Object, _settings);

    [Fact]
    public async Task HandleAsync_WithNullToken_ReturnsInvalidRefreshToken()
    {
        var result = await CreateService().HandleAsync(null, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("INVALID_REFRESH_TOKEN");
    }

    [Fact]
    public async Task HandleAsync_WithUnknownHash_ReturnsInvalidRefreshToken()
    {
        _refreshRepo.Setup(r => r.GetByHashAsync(It.IsAny<string>(), default))
                    .ReturnsAsync((RefreshTokenData?)null);

        var result = await CreateService().HandleAsync("unknown-token", default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("INVALID_REFRESH_TOKEN");
    }

    [Fact]
    public async Task HandleAsync_WithExpiredToken_ReturnsInvalidRefreshToken()
    {
        var expired = new RefreshTokenData(
            Guid.NewGuid(), Guid.NewGuid(), "hash",
            ExpiresAt: DateTime.UtcNow.AddDays(-1),
            RevokedAt: null);
        _refreshRepo.Setup(r => r.GetByHashAsync(It.IsAny<string>(), default)).ReturnsAsync(expired);

        var result = await CreateService().HandleAsync("some-token", default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("INVALID_REFRESH_TOKEN");
    }

    [Fact]
    public async Task HandleAsync_WithAlreadyRevokedToken_RevokesAllUserTokensAndReturnsError()
    {
        var userId = Guid.NewGuid();
        var revoked = new RefreshTokenData(
            Guid.NewGuid(), userId, "hash",
            ExpiresAt: DateTime.UtcNow.AddDays(10),
            RevokedAt: DateTime.UtcNow.AddHours(-1));
        _refreshRepo.Setup(r => r.GetByHashAsync(It.IsAny<string>(), default)).ReturnsAsync(revoked);
        _refreshRepo.Setup(r => r.RevokeAllForUserAsync(userId, default)).Returns(Task.CompletedTask);

        var result = await CreateService().HandleAsync("reused-token", default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("INVALID_REFRESH_TOKEN");
        _refreshRepo.Verify(r => r.RevokeAllForUserAsync(userId, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithValidToken_RevokesOldTokenAndIssuesNewPair()
    {
        var userId = Guid.NewGuid();
        var tokenId = Guid.NewGuid();
        var stored = new RefreshTokenData(
            tokenId, userId, "hash",
            ExpiresAt: DateTime.UtcNow.AddDays(10),
            RevokedAt: null);
        var user = new UserData(userId, "user@example.com", "hash", "John Doe");

        _refreshRepo.Setup(r => r.GetByHashAsync(It.IsAny<string>(), default)).ReturnsAsync(stored);
        _refreshRepo.Setup(r => r.RevokeAsync(tokenId, default)).Returns(Task.CompletedTask);
        _refreshRepo.Setup(r => r.StoreAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), default))
                    .Returns(Task.CompletedTask);
        _userRepo.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateAccessToken(userId, "user@example.com", "John Doe")).Returns("new-access-token");

        var result = await CreateService().HandleAsync("valid-raw-token", default);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
        result.Value.RawRefreshToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
        _refreshRepo.Verify(r => r.RevokeAsync(tokenId, default), Times.Once);
        _refreshRepo.Verify(r => r.StoreAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>(), default), Times.Once);
    }
}
