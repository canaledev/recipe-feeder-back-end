namespace Feedy.Application.Tests.UseCases.GetSubscribedPlaylists;

using Feedy.Application.UseCases.GetSubscribedPlaylists;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

public class GetSubscribedPlaylistsServiceTests
{
    private readonly Mock<IPlaylistRepository> _repo = new();

    private GetSubscribedPlaylistsService CreateService() => new(_repo.Object);

    private static PlaylistData MakePlaylist() => new(
        Guid.NewGuid(), "Title", "Desc", "http://img", "Author",
        3, 20.0, "Easy", 4.0, true, DateTime.UtcNow);

    [Fact]
    public async Task HandleAsync_ReturnsSubscribedPlaylistsFromRepository()
    {
        var userId = Guid.NewGuid();
        var playlists = new[] { MakePlaylist(), MakePlaylist() };
        _repo.Setup(r => r.GetSubscribedByUserAsync(userId, null, null, null, "subscriptionDate", default))
             .ReturnsAsync(playlists);

        var result = await CreateService().HandleAsync(
            new GetSubscribedPlaylistsRequest(userId, null, null, null, "subscriptionDate"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_PassesAllFiltersToRepository()
    {
        var userId = Guid.NewGuid();
        _repo.Setup(r => r.GetSubscribedByUserAsync(userId, "vegan", "Easy", "inProgress", "alphabetical", default))
             .ReturnsAsync(Enumerable.Empty<PlaylistData>());

        await CreateService().HandleAsync(
            new GetSubscribedPlaylistsRequest(userId, "vegan", "Easy", "inProgress", "alphabetical"), default);

        _repo.Verify(r => r.GetSubscribedByUserAsync(userId, "vegan", "Easy", "inProgress", "alphabetical", default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoSubscriptions_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();
        _repo.Setup(r => r.GetSubscribedByUserAsync(userId, null, null, null, "subscriptionDate", default))
             .ReturnsAsync(Enumerable.Empty<PlaylistData>());

        var result = await CreateService().HandleAsync(
            new GetSubscribedPlaylistsRequest(userId, null, null, null, "subscriptionDate"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }
}
