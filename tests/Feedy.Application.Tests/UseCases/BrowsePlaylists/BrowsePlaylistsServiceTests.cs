namespace Feedy.Application.Tests.UseCases.BrowsePlaylists;

using Feedy.Application.UseCases.BrowsePlaylists;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

public class BrowsePlaylistsServiceTests
{
    private readonly Mock<IPlaylistRepository> _repo = new();

    private BrowsePlaylistsService CreateService() => new(_repo.Object);

    private static PlaylistData MakePlaylist(Guid? id = null) => new(
        id ?? Guid.NewGuid(), "Title", "Desc", "http://img", "Author",
        5, 30.0, "Easy", 4.2, false, DateTime.UtcNow);

    [Fact]
    public async Task HandleAsync_ReturnsPagedResultFromRepository()
    {
        var playlists = new[] { MakePlaylist(), MakePlaylist() };
        _repo.Setup(r => r.GetAllAsync(null, null, null, "relevance", 1, 10, null, default))
             .ReturnsAsync((playlists, 2));

        var request = new BrowsePlaylistsRequest(null, null, null, "relevance", 1, 10);
        var result = await CreateService().HandleAsync(request, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Total.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task HandleAsync_PassesAllFiltersToRepository()
    {
        _repo.Setup(r => r.GetAllAsync("pasta", "vegan", "Easy", "popularity", 2, 5, null, default))
             .ReturnsAsync((Enumerable.Empty<PlaylistData>(), 0));

        var request = new BrowsePlaylistsRequest("pasta", "vegan", "Easy", "popularity", 2, 5);
        await CreateService().HandleAsync(request, default);

        _repo.Verify(r => r.GetAllAsync("pasta", "vegan", "Easy", "popularity", 2, 5, null, default), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryReturnsEmpty_ReturnsEmptyPage()
    {
        _repo.Setup(r => r.GetAllAsync(null, null, null, "relevance", 1, 10, null, default))
             .ReturnsAsync((Enumerable.Empty<PlaylistData>(), 0));

        var result = await CreateService().HandleAsync(
            new BrowsePlaylistsRequest(null, null, null, "relevance", 1, 10), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.Total.Should().Be(0);
    }
}
