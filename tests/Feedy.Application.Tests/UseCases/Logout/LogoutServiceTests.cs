namespace Feedy.Application.Tests.UseCases.Logout;

using Feedy.Application.UseCases.Logout;
using Feedy.Domain.Interfaces;
using Moq;
using Xunit;

public class LogoutServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshRepo = new();

    private LogoutService CreateService() => new(_refreshRepo.Object);

    [Fact]
    public async Task HandleAsync_RevokesAllRefreshTokensForUser()
    {
        var userId = Guid.NewGuid();
        _refreshRepo.Setup(r => r.RevokeAllForUserAsync(userId, default)).Returns(Task.CompletedTask);

        await CreateService().HandleAsync(userId, default);

        _refreshRepo.Verify(r => r.RevokeAllForUserAsync(userId, default), Times.Once);
    }
}
