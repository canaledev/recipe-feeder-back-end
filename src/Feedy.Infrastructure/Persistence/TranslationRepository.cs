namespace Feedy.Infrastructure.Persistence;

using Dapper;
using Feedy.Domain.Interfaces;
using Npgsql;

/// <summary>
/// Dapper implementation of ITranslationRepository.
/// Queries the generic translations table that stores translated content for all entity types.
/// </summary>
public class TranslationRepository : ITranslationRepository
{
    private readonly string _connectionString;

    public TranslationRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<string?> GetAsync(
        string entityType,
        Guid entityId,
        string fieldName,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT value
            FROM translations
            WHERE entity_type    = @EntityType
              AND entity_id      = @EntityId
              AND field_name     = @FieldName
              AND language_code  = @LanguageCode
            LIMIT 1";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<string>(sql, new
        {
            EntityType   = entityType,
            EntityId     = entityId,
            FieldName    = fieldName,
            LanguageCode = languageCode
        });
    }

    public async Task<IReadOnlyDictionary<string, string>> GetManyAsync(
        string entityType,
        Guid entityId,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT field_name, value
            FROM translations
            WHERE entity_type   = @EntityType
              AND entity_id     = @EntityId
              AND language_code = @LanguageCode";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<(string FieldName, string Value)>(sql, new
        {
            EntityType   = entityType,
            EntityId     = entityId,
            LanguageCode = languageCode
        });

        return rows.ToDictionary(r => r.FieldName, r => r.Value);
    }
}
