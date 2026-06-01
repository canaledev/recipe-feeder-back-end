namespace Feedy.Application.Tests.UseCases.SubscribeToPlaylist;

using Feedy.Application.UseCases.SubscribeToPlaylist;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

public class SubscribeToPlaylistServiceTests
{
    private readonly Mock<IPlaylistRepository> _repo = new();

    private SubscribeToPlaylistService CreateService() => new(_repo.Object);

    [Fact]
    public async Task HandleAsync_WhenAlreadySubscribed_ReturnsAlreadySubscribedError()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        _repo.Setup(r => r.IsSubscribedAsync(userId, playlistId, default)).ReturnsAsync(true);

        var result = await CreateService().HandleAsync(
            new SubscribeToPlaylistRequest(userId, playlistId), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("ALREADY_SUBSCRIBED");
    }

    [Fact]
    public async Task HandleAsync_WhenNotSubscribed_CallsSubscribeAsync()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        _repo.Setup(r => r.IsSubscribedAsync(userId, playlistId, default)).ReturnsAsync(false);
        _repo.Setup(r => r.SubscribeAsync(userId, playlistId, default)).Returns(Task.CompletedTask);

        var result = await CreateService().HandleAsync(
            new SubscribeToPlaylistRequest(userId, playlistId), default);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.SubscribeAsync(userId, playlistId, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadySubscribed_DoesNotCallSubscribeAsync()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        _repo.Setup(r => r.IsSubscribedAsync(userId, playlistId, default)).ReturnsAsync(true);

        await CreateService().HandleAsync(
            new SubscribeToPlaylistRequest(userId, playlistId), default);

        _repo.Verify(r => r.SubscribeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default), Times.Never);
    }
}
