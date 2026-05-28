namespace Feedy.Application.Tests.UseCases.LoginUser;

using Feedy.Application.Configuration;
using Feedy.Application.Interfaces;
using Feedy.Application.UseCases.LoginUser;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class LoginUserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenProvider> _jwt = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();
    private readonly IOptions<JwtSettings> _settings =
        Options.Create(new JwtSettings { AccessTokenExpirationMinutes = 15, RefreshTokenExpirationDays = 30 });

    private LoginUserService CreateService() =>
        new(_repo.Object, _hasher.Object, _jwt.Object, _refreshRepo.Object, _settings);

    [Fact]
    public async Task HandleAsync_WhenEmailNotFound_ReturnsInvalidCredentials()
    {
        _repo.Setup(r => r.GetByEmailAsync("unknown@example.com", default))
             .ReturnsAsync((UserData?)null);

        var result = await CreateService().HandleAsync(
            new LoginUserRequest("unknown@example.com", "Password1!"), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordWrong_ReturnsInvalidCredentials()
    {
        var user = new UserData(Guid.NewGuid(), "user@example.com", "stored-hash", "User");
        _repo.Setup(r => r.GetByEmailAsync("user@example.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("WrongPassword", "stored-hash")).Returns(false);

        var result = await CreateService().HandleAsync(
            new LoginUserRequest("user@example.com", "WrongPassword"), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task HandleAsync_WhenEmailNotFound_AndWhenPasswordWrong_ReturnIdenticalError()
    {
        _repo.Setup(r => r.GetByEmailAsync("unknown@example.com", default))
             .ReturnsAsync((UserData?)null);
        var knownUser = new UserData(Guid.NewGuid(), "known@example.com", "hash", "User");
        _repo.Setup(r => r.GetByEmailAsync("known@example.com", default)).ReturnsAsync(knownUser);
        _hasher.Setup(h => h.Verify("WrongPassword", "hash")).Returns(false);

        var resultUnknown = await CreateService().HandleAsync(
            new LoginUserRequest("unknown@example.com", "WrongPassword"), default);
        var resultWrongPwd = await CreateService().HandleAsync(
            new LoginUserRequest("known@example.com", "WrongPassword"), default);

        resultUnknown.Error.Code.Should().Be(resultWrongPwd.Error.Code);
        resultUnknown.Error.Message.Should().Be(resultWrongPwd.Error.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenCredentialsValid_GeneratesAccessToken()
    {
        var userId = Guid.NewGuid();
        var user = new UserData(userId, "user@example.com", "stored-hash", "John Doe");
        _repo.Setup(r => r.GetByEmailAsync("user@example.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Password1!", "stored-hash")).Returns(true);
        _refreshRepo.Setup(r => r.StoreAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), default))
                    .Returns(Task.CompletedTask);
        _jwt.Setup(j => j.GenerateAccessToken(userId, "user@example.com", "John Doe")).Returns("access-token");

        await CreateService().HandleAsync(
            new LoginUserRequest("user@example.com", "Password1!"), default);

        _jwt.Verify(j => j.GenerateAccessToken(userId, "user@example.com", "John Doe"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCredentialsValid_StoresRefreshTokenHash()
    {
        var userId = Guid.NewGuid();
        var user = new UserData(userId, "user@example.com", "stored-hash", "John Doe");
        _repo.Setup(r => r.GetByEmailAsync("user@example.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Password1!", "stored-hash")).Returns(true);
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access-token");
        _refreshRepo.Setup(r => r.StoreAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), default))
                    .Returns(Task.CompletedTask);

        await CreateService().HandleAsync(
            new LoginUserRequest("user@example.com", "Password1!"), default);

        _refreshRepo.Verify(r => r.StoreAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>(), default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCredentialsValid_ReturnsAccessTokenAndUserData()
    {
        var userId = Guid.NewGuid();
        var user = new UserData(userId, "user@example.com", "stored-hash", "John Doe");
        _repo.Setup(r => r.GetByEmailAsync("user@example.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Password1!", "stored-hash")).Returns(true);
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access-token");
        _refreshRepo.Setup(r => r.StoreAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), default))
                    .Returns(Task.CompletedTask);

        var result = await CreateService().HandleAsync(
            new LoginUserRequest("user@example.com", "Password1!"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.UserId.Should().Be(userId);
        result.Value.Email.Should().Be("user@example.com");
        result.Value.FullName.Should().Be("John Doe");
        result.Value.RawRefreshToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
