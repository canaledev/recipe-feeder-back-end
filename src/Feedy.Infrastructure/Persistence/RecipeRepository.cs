namespace Feedy.Infrastructure.Persistence;

using Dapper;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using Npgsql;

/// <summary>
/// Recipe repository implementation using Dapper and PostgreSQL.
/// </summary>
internal class RecipeRepository : IRecipeRepository
{
    private readonly string _connectionString;

    public RecipeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<RecipeData>>
        GetRecipesForFeedAsync(IEnumerable<string> preferredTags, IEnumerable<string> dietaryRegime, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT id, title, description, tags, difficulty,
                   prep_time_minutes  AS PrepTimeMinutes,
                   main_image_url     AS MainImageUrl,
                   source_language    AS SourceLanguage
            FROM recipes
            WHERE is_active = true
            LIMIT 100";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var recipes = await connection.QueryAsync<RecipeData>(sql);
        return recipes;
    }

    public async Task<(Guid Id, string Title, string Description, string[] Tags, string Difficulty, int PrepTimeMinutes, string[] Ingredients, string MainImageUrl)?>
        GetRecipeByIdAsync(Guid recipeId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT r.id, r.title, r.description, r.tags, r.difficulty, r.prep_time_minutes, r.ingredients, r.main_image_url
            FROM recipes r
            WHERE r.id = @RecipeId AND r.is_active = true";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var recipe = await connection.QueryFirstOrDefaultAsync<(Guid, string, string, string[], string, int, string[], string)>(
            sql,
            new { RecipeId = recipeId });

        return recipe == default ? null : recipe;
    }
}
