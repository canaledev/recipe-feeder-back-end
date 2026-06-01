namespace Feedy.Application.Tests.UseCases.UnsubscribeFromPlaylist;

using Feedy.Application.UseCases.UnsubscribeFromPlaylist;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

public class UnsubscribeFromPlaylistServiceTests
{
    private readonly Mock<IPlaylistRepository> _repo = new();

    private UnsubscribeFromPlaylistService CreateService() => new(_repo.Object);

    [Fact]
    public async Task HandleAsync_WhenNotSubscribed_ReturnsNotSubscribedError()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        _repo.Setup(r => r.IsSubscribedAsync(userId, playlistId, default)).ReturnsAsync(false);

        var result = await CreateService().HandleAsync(
            new UnsubscribeFromPlaylistRequest(userId, playlistId), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("NOT_SUBSCRIBED");
    }

    [Fact]
    public async Task HandleAsync_WhenSubscribed_CallsUnsubscribeAsync()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        _repo.Setup(r => r.IsSubscribedAsync(userId, playlistId, default)).ReturnsAsync(true);
        _repo.Setup(r => r.UnsubscribeAsync(userId, playlistId, default)).Returns(Task.CompletedTask);

        var result = await CreateService().HandleAsync(
            new UnsubscribeFromPlaylistRequest(userId, playlistId), default);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.UnsubscribeAsync(userId, playlistId, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNotSubscribed_DoesNotCallUnsubscribeAsync()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        _repo.Setup(r => r.IsSubscribedAsync(userId, playlistId, default)).ReturnsAsync(false);

        await CreateService().HandleAsync(
            new UnsubscribeFromPlaylistRequest(userId, playlistId), default);

        _repo.Verify(r => r.UnsubscribeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default), Times.Never);
    }
}
