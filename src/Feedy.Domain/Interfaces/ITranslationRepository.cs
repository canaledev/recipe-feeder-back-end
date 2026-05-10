namespace Feedy.Domain.Interfaces;

/// <summary>
/// Repository for the generic translations table. Implemented by Infrastructure layer.
/// </summary>
public interface ITranslationRepository
{
    /// <summary>
    /// Gets a single translated field value for an entity.
    /// Returns null if no translation exists for the given language.
    /// </summary>
    Task<string?> GetAsync(
        string entityType,
        Guid entityId,
        string fieldName,
        string languageCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all translated fields for an entity in a given language.
    /// Returns an empty dictionary if no translations exist.
    /// </summary>
    Task<IReadOnlyDictionary<string, string>> GetManyAsync(
        string entityType,
        Guid entityId,
        string languageCode,
        CancellationToken cancellationToken = default);
}
