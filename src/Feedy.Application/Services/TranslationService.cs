namespace Feedy.Application.Services;

using Feedy.Application.Interfaces;
using Feedy.Domain.Interfaces;
using Microsoft.Extensions.Localization;

/// <summary>
/// Resolves translated content fields and error messages.
///
/// Content fallback chain (Chain of Responsibility):
///   1. requestedLanguage — look up in translations table
///   2. sourceLanguage    — look up in translations table
///   3. en               — look up in translations table (skipped if source is already en)
///   4. originalValue    — return as-is
///
/// Error messages use IStringLocalizer, which reads .resx files and respects
/// CultureInfo.CurrentCulture set by RequestLocalizationMiddleware.
/// </summary>
public class TranslationService : ITranslationService
{
    private readonly ITranslationRepository _repository;
    private readonly IStringLocalizer<TranslationService> _localizer;

    public TranslationService(
        ITranslationRepository repository,
        IStringLocalizer<TranslationService> localizer)
    {
        _repository = repository;
        _localizer = localizer;
    }

    public async Task<string> GetContentAsync(
        string entityType,
        Guid entityId,
        string fieldName,
        string sourceLanguage,
        string originalValue,
        string requestedLanguage,
        CancellationToken cancellationToken = default)
    {
        // Short-circuit: content is already in the requested language
        if (requestedLanguage == sourceLanguage)
            return originalValue;

        // Step 1: try requested language
        var translation = await _repository.GetAsync(entityType, entityId, fieldName, requestedLanguage, cancellationToken);
        if (translation is not null)
            return translation;

        // Step 2: try source language
        translation = await _repository.GetAsync(entityType, entityId, fieldName, sourceLanguage, cancellationToken);
        if (translation is not null)
            return translation;

        // Step 3: try English — only if source is not already English (already tried above)
        if (sourceLanguage != "en")
        {
            translation = await _repository.GetAsync(entityType, entityId, fieldName, "en", cancellationToken);
            if (translation is not null)
                return translation;
        }

        // Step 4: no translation found — return the original value unchanged
        return originalValue;
    }

    public string GetErrorMessage(string errorCode)
    {
        var localized = _localizer[errorCode];
        return localized.ResourceNotFound ? errorCode : localized.Value;
    }
}
