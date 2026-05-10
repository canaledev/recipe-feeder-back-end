namespace Feedy.Application.Tests.Services;

using Feedy.Application.Services;
using Xunit;
using Feedy.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Localization;
using Moq;

public class TranslationServiceTests
{
    private readonly Mock<ITranslationRepository> _repoMock = new();
    private readonly Mock<IStringLocalizer<TranslationService>> _localizerMock = new();
    private readonly TranslationService _sut;
    private readonly Guid _entityId = Guid.NewGuid();

    public TranslationServiceTests()
    {
        _sut = new TranslationService(_repoMock.Object, _localizerMock.Object);
    }

    // --- GetContentAsync fallback chain ---

    [Fact]
    public async Task GetContentAsync_ReturnsOriginalValue_WhenRequestedLanguageEqualsSourceLanguage()
    {
        var result = await _sut.GetContentAsync("recipe", _entityId, "title", "es", "Arroz con leche", "es");

        result.Should().Be("Arroz con leche");
        _repoMock.Verify(
            r => r.GetAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetContentAsync_ReturnsRequestedLanguageTranslation_WhenFound()
    {
        _repoMock
            .Setup(r => r.GetAsync("recipe", _entityId, "title", "fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync("Riz au lait");

        var result = await _sut.GetContentAsync("recipe", _entityId, "title", "es", "Arroz con leche", "fr");

        result.Should().Be("Riz au lait");
    }

    [Fact]
    public async Task GetContentAsync_FallsBackToSourceLanguage_WhenRequestedLanguageMissing()
    {
        _repoMock
            .Setup(r => r.GetAsync("recipe", _entityId, "title", "fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _repoMock
            .Setup(r => r.GetAsync("recipe", _entityId, "title", "es", It.IsAny<CancellationToken>()))
            .ReturnsAsync("Arroz con leche");

        var result = await _sut.GetContentAsync("recipe", _entityId, "title", "es", "Arroz con leche", "fr");

        result.Should().Be("Arroz con leche");
    }

    [Fact]
    public async Task GetContentAsync_FallsBackToEnglish_WhenRequestedAndSourceLanguageMissing()
    {
        _repoMock
            .Setup(r => r.GetAsync("recipe", _entityId, "title", "fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _repoMock
            .Setup(r => r.GetAsync("recipe", _entityId, "title", "es", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _repoMock
            .Setup(r => r.GetAsync("recipe", _entityId, "title", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync("Rice pudding");

        var result = await _sut.GetContentAsync("recipe", _entityId, "title", "es", "Arroz con leche", "fr");

        result.Should().Be("Rice pudding");
    }

    [Fact]
    public async Task GetContentAsync_ReturnsOriginalValue_WhenNoTranslationsFound()
    {
        _repoMock
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await _sut.GetContentAsync("recipe", _entityId, "title", "es", "Arroz con leche", "fr");

        result.Should().Be("Arroz con leche");
    }

    [Fact]
    public async Task GetContentAsync_DoesNotDuplicateEnglishLookup_WhenSourceLanguageIsEnglish()
    {
        // source = en, requested = fr: chain tries fr (miss), then en as source (miss) — must NOT try en a second time
        _repoMock
            .Setup(r => r.GetAsync("recipe", _entityId, "title", "fr", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
        _repoMock
            .Setup(r => r.GetAsync("recipe", _entityId, "title", "en", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var result = await _sut.GetContentAsync("recipe", _entityId, "title", "en", "Rice pudding", "fr");

        result.Should().Be("Rice pudding");
        _repoMock.Verify(
            r => r.GetAsync("recipe", _entityId, "title", "en", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // --- GetErrorMessage ---

    [Fact]
    public void GetErrorMessage_ReturnsLocalizedMessage_WhenTranslationFound()
    {
        var localizedString = new LocalizedString("USER_ALREADY_EXISTS", "El usuario ya existe");
        _localizerMock.Setup(l => l["USER_ALREADY_EXISTS"]).Returns(localizedString);

        var result = _sut.GetErrorMessage("USER_ALREADY_EXISTS");

        result.Should().Be("El usuario ya existe");
    }

    [Fact]
    public void GetErrorMessage_ReturnsErrorCode_WhenTranslationNotFound()
    {
        var localizedString = new LocalizedString("UNKNOWN_CODE", "UNKNOWN_CODE", resourceNotFound: true);
        _localizerMock.Setup(l => l["UNKNOWN_CODE"]).Returns(localizedString);

        var result = _sut.GetErrorMessage("UNKNOWN_CODE");

        result.Should().Be("UNKNOWN_CODE");
    }
}
