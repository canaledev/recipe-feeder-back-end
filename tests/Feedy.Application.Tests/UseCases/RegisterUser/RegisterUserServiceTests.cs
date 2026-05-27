namespace Feedy.Application.Tests.UseCases.RegisterUser;

using Feedy.Application.Interfaces;
using Xunit;
using Feedy.Application.UseCases.RegisterUser;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Moq;

public class RegisterUserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtTokenProvider> _jwt = new();

    private RegisterUserService CreateService() =>
        new(_repo.Object, _hasher.Object, _jwt.Object);

    [Fact]
    public async Task HandleAsync_WhenEmailAlreadyExists_ReturnsDuplicateEmailError()
    {
        var existingUser = new UserData(Guid.NewGuid(), "user@example.com", "hash", "Existing User");
        _repo.Setup(r => r.GetByEmailAsync("user@example.com", default))
             .ReturnsAsync(existingUser);

        var request = new RegisterUserRequest("user@example.com", "Password1!", "New User");
        var result = await CreateService().HandleAsync(request, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("DUPLICATE_EMAIL");
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_HashesPassword()
    {
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
             .ReturnsAsync((UserData?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
             .ReturnsAsync(Guid.NewGuid());
        _hasher.Setup(h => h.Hash("Password1!")).Returns("bcrypt-hash");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("jwt-token");

        var request = new RegisterUserRequest("new@example.com", "Password1!", "New User");
        await CreateService().HandleAsync(request, default);

        _hasher.Verify(h => h.Hash("Password1!"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_CallsCreateAsyncWithFullName()
    {
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
             .ReturnsAsync((UserData?)null);
        _repo.Setup(r => r.CreateAsync("new@example.com", "bcrypt-hash", "New User", default))
             .ReturnsAsync(Guid.NewGuid());
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("bcrypt-hash");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("jwt-token");

        var request = new RegisterUserRequest("new@example.com", "Password1!", "New User");
        await CreateService().HandleAsync(request, default);

        _repo.Verify(r => r.CreateAsync("new@example.com", "bcrypt-hash", "New User", default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_GeneratesJwtToken()
    {
        var userId = Guid.NewGuid();
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
             .ReturnsAsync((UserData?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
             .ReturnsAsync(userId);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("bcrypt-hash");
        _jwt.Setup(j => j.GenerateToken(userId, "new@example.com", "New User"))
            .Returns("jwt-token");

        var request = new RegisterUserRequest("new@example.com", "Password1!", "New User");
        await CreateService().HandleAsync(request, default);

        _jwt.Verify(j => j.GenerateToken(userId, "new@example.com", "New User"), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNew_ReturnsTokenAndUserData()
    {
        var userId = Guid.NewGuid();
        _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
             .ReturnsAsync((UserData?)null);
        _repo.Setup(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), default))
             .ReturnsAsync(userId);
        _hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("bcrypt-hash");
        _jwt.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("jwt-token");

        var request = new RegisterUserRequest("new@example.com", "Password1!", "New User");
        var result = await CreateService().HandleAsync(request, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("jwt-token");
        result.Value.UserId.Should().Be(userId);
        result.Value.Email.Should().Be("new@example.com");
        result.Value.FullName.Should().Be("New User");
    }
}
