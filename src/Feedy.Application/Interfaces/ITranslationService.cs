namespace Feedy.Application.Interfaces;

/// <summary>
/// Application service for resolving translated content and error messages.
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// Resolves a translated field value using the fallback chain:
    /// requestedLanguage → sourceLanguage → en → originalValue.
    /// </summary>
    Task<string> GetContentAsync(
        string entityType,
        Guid entityId,
        string fieldName,
        string sourceLanguage,
        string originalValue,
        string requestedLanguage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a localized error message for the given error code using the current request culture.
    /// Falls back to the error code itself if no translation is found.
    /// </summary>
    string GetErrorMessage(string errorCode);
}
