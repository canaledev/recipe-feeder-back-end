namespace Feedy.Application.Tests.UseCases.RegisterUser;

using Feedy.Application.Configuration;
using Feedy.Application.Interfaces;
using Feedy.Application.UseCases.RegisterUser;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class RegisterUserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenProvider> _jwt = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();
    private readonly IOptions<JwtSettings> _settings =
        Options.Create(new JwtSettings { AccessTokenExpirationMinutes = 15, RefreshTokenExpirationDays = 30 });

    private RegisterUserService CreateService() =>
        new(_repo.Object, _hasher.Object, _jwt.Object, _refreshRepo.Object, _settings);

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyExists_ReturnsDuplicateEmailError()
    {
        var existingUser = new UserData(Guid.NewGuid(), "user@example.com", "hash", "Existing User");
        _repo.Setup(r => r.GetByEmailAsync("user@example.com", default)).ReturnsAsync(existingUser);

        var result = await CreateService().HandleAsync(
            new RegisterUserRequest("user@example.com", "Password1!", "New User"), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("DUPLICATE_EMAIL");
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_HashesPassword()
    {
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((UserData?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
             .ReturnsAsync(Guid.NewGuid());
        _hasher.Setup(h => h.Hash("Password1!")).Returns("bcrypt-hash");
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access-token");
        _refreshRepo.Setup(r => r.StoreAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), default))
                    .Returns(Task.CompletedTask);

        await CreateService().HandleAsync(
            new RegisterUserRequest("new@example.com", "Password1!", "New User"), default);

        _hasher.Verify(h => h.Hash("Password1!"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_CallsCreateAsyncWithFullName()
    {
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((UserData?)null);
        _repo.Setup(r => r.CreateAsync("new@example.com", "bcrypt-hash", "New User", default))
             .ReturnsAsync(Guid.NewGuid());
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("bcrypt-hash");
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access-token");
        _refreshRepo.Setup(r => r.StoreAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), default))
                    .Returns(Task.CompletedTask);

        await CreateService().HandleAsync(
            new RegisterUserRequest("new@example.com", "Password1!", "New User"), default);

        _repo.Verify(r => r.CreateAsync("new@example.com", "bcrypt-hash", "New User", default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_GeneratesAccessToken()
    {
        var userId = Guid.NewGuid();
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((UserData?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
             .ReturnsAsync(userId);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("bcrypt-hash");
        _jwt.Setup(j => j.GenerateAccessToken(userId, "new@example.com", "New User")).Returns("access-token");
        _refreshRepo.Setup(r => r.StoreAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), default))
                    .Returns(Task.CompletedTask);

        await CreateService().HandleAsync(
            new RegisterUserRequest("new@example.com", "Password1!", "New User"), default);

        _jwt.Verify(j => j.GenerateAccessToken(userId, "new@example.com", "New User"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_StoresRefreshTokenHash()
    {
        var userId = Guid.NewGuid();
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((UserData?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
             .ReturnsAsync(userId);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("bcrypt-hash");
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access-token");
        _refreshRepo.Setup(r => r.StoreAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), default))
                    .Returns(Task.CompletedTask);

        await CreateService().HandleAsync(
            new RegisterUserRequest("new@example.com", "Password1!", "New User"), default);

        _refreshRepo.Verify(r => r.StoreAsync(userId, It.IsAny<string>(), It.IsAny<DateTime>(), default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_ReturnsAccessTokenAndUserData()
    {
        var userId = Guid.NewGuid();
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((UserData?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
             .ReturnsAsync(userId);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("bcrypt-hash");
        _jwt.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access-token");
        _refreshRepo.Setup(r => r.StoreAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>(), default))
                    .Returns(Task.CompletedTask);

        var result = await CreateService().HandleAsync(
            new RegisterUserRequest("new@example.com", "Password1!", "New User"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.UserId.Should().Be(userId);
        result.Value.Email.Should().Be("new@example.com");
        result.Value.FullName.Should().Be("New User");
        result.Value.RawRefreshToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshTokenExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
