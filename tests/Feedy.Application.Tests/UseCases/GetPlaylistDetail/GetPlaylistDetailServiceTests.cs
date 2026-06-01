namespace Feedy.Application.Tests.UseCases.GetPlaylistDetail;

using Feedy.Application.UseCases.GetPlaylistDetail;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

public class GetPlaylistDetailServiceTests
{
    private readonly Mock<IPlaylistRepository> _repo = new();
    private readonly Mock<IUserRepository> _userRepo = new();

    private GetPlaylistDetailService CreateService() =>
        new(_repo.Object, _userRepo.Object);

    private static PlaylistData MakePlaylist(Guid id) => new(
        id, "Title", "Desc", "http://img", "Author",
        2, 25.0, "Easy", 4.5, true, DateTime.UtcNow);

    private static PlaylistItemData MakeItem(string[] tags) => new(
        Guid.NewGuid(), "Recipe", "http://thumb", 20, "Easy", tags, "never_done");

    [Fact]
    public async Task HandleAsync_WhenPlaylistNotFound_ReturnsPlaylistNotFoundError()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(playlistId, userId, default))
             .ReturnsAsync((PlaylistData?)null);

        var result = await CreateService().HandleAsync(
            new GetPlaylistDetailRequest(playlistId, userId), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("PLAYLIST_NOT_FOUND");
    }

    [Fact]
    public async Task HandleAsync_WhenFound_ReturnsPlaylistDetailWithItems()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var playlist = MakePlaylist(playlistId);
        var items = new[] { MakeItem(["italian"]), MakeItem(["vegan"]) };
        var profile = new UserProfileData(userId, ["italian"], [], []);

        _repo.Setup(r => r.GetByIdAsync(playlistId, userId, default)).ReturnsAsync(playlist);
        _repo.Setup(r => r.GetItemsAsync(playlistId, userId, default)).ReturnsAsync(items);
        _userRepo.Setup(r => r.GetProfileAsync(userId, default)).ReturnsAsync(profile);

        var result = await CreateService().HandleAsync(
            new GetPlaylistDetailRequest(playlistId, userId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(playlistId);
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_ComputesMatchPercentagePerItemFromUserProfile()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var playlist = MakePlaylist(playlistId);
        // Item tags match user tags → score > 50; item with no match → 50
        var items = new[]
        {
            MakeItem(["italian", "spicy"]),
            MakeItem(["vegan"])
        };
        var profile = new UserProfileData(userId, ["italian", "spicy"], [], []);

        _repo.Setup(r => r.GetByIdAsync(playlistId, userId, default)).ReturnsAsync(playlist);
        _repo.Setup(r => r.GetItemsAsync(playlistId, userId, default)).ReturnsAsync(items);
        _userRepo.Setup(r => r.GetProfileAsync(userId, default)).ReturnsAsync(profile);

        var result = await CreateService().HandleAsync(
            new GetPlaylistDetailRequest(playlistId, userId), default);

        var itemsList = result.Value.Items.ToList();
        // 2 matching tags → 50 + 20 = 70
        itemsList[0].MatchPercentage.Should().Be(70);
        // 0 matching tags → 50
        itemsList[1].MatchPercentage.Should().Be(50);
    }

    [Fact]
    public async Task HandleAsync_WhenProfileNotFound_DefaultsMatchPercentageToBase()
    {
        var userId = Guid.NewGuid();
        var playlistId = Guid.NewGuid();
        var playlist = MakePlaylist(playlistId);
        var items = new[] { MakeItem(["italian"]) };

        _repo.Setup(r => r.GetByIdAsync(playlistId, userId, default)).ReturnsAsync(playlist);
        _repo.Setup(r => r.GetItemsAsync(playlistId, userId, default)).ReturnsAsync(items);
        _userRepo.Setup(r => r.GetProfileAsync(userId, default))
                 .ReturnsAsync((UserProfileData?)null);

        var result = await CreateService().HandleAsync(
            new GetPlaylistDetailRequest(playlistId, userId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.First().MatchPercentage.Should().Be(50);
    }
}
