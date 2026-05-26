namespace Feedy.Application.Tests.UseCases.LoginUser;

using Feedy.Application.Interfaces;
using Xunit;
using Feedy.Application.UseCases.LoginUser;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Moq;

public class LoginUserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenProvider> _jwt = new();

    private LoginUserService CreateService() =>
        new(_repo.Object, _hasher.Object, _jwt.Object);

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
        // Anti-enumeration: same Code and Message regardless of failure branch
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
    public async Task HandleAsync_WhenCredentialsValid_GeneratesToken()
    {
        var userId = Guid.NewGuid();
        var user = new UserData(userId, "user@example.com", "stored-hash", "John Doe");
        _repo.Setup(r => r.GetByEmailAsync("user@example.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Password1!", "stored-hash")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(userId, "user@example.com", "John Doe")).Returns("jwt-token");

        await CreateService().HandleAsync(
            new LoginUserRequest("user@example.com", "Password1!"), default);

        _jwt.Verify(j => j.GenerateToken(userId, "user@example.com", "John Doe"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCredentialsValid_ReturnsTokenAndUserData()
    {
        var userId = Guid.NewGuid();
        var user = new UserData(userId, "user@example.com", "stored-hash", "John Doe");
        _repo.Setup(r => r.GetByEmailAsync("user@example.com", default)).ReturnsAsync(user);
        _hasher.Setup(h => h.Verify("Password1!", "stored-hash")).Returns(true);
        _jwt.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("jwt-token");

        var result = await CreateService().HandleAsync(
            new LoginUserRequest("user@example.com", "Password1!"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("jwt-token");
        result.Value.UserId.Should().Be(userId);
        result.Value.Email.Should().Be("user@example.com");
        result.Value.FullName.Should().Be("John Doe");
    }
}
