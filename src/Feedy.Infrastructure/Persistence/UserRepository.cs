namespace Feedy.Infrastructure.Persistence;

using Dapper;
using Feedy.Domain.Entities;
using Feedy.Domain.Interfaces;
using Npgsql;

/// <summary>
/// User repository implementation using Dapper and PostgreSQL.
/// </summary>
internal class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<UserData?>
        GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT u.id, u.email, u.password_hash AS PasswordHash, u.full_name AS FullName
            FROM users u
            WHERE u.email = @Email";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var user = await connection.QueryFirstOrDefaultAsync<UserData>(
            sql,
            new { Email = email });

        return user;
    }

    public async Task<UserProfileData?>
        GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT user_id as UserId, flavor_tags as FlavorTags, dietary_regime as DietaryRegime, rejected_ingredients as RejectedIngredients
            FROM user_profiles
            WHERE user_id = @UserId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var profile = await connection.QueryFirstOrDefaultAsync<UserProfileData>(
            sql,
            new { UserId = userId });

        return profile;
    }

    public async Task<Guid> CreateAsync(string email, string passwordHash, string fullName, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO users (id, email, password_hash, full_name, created_at)
            VALUES (@Id, @Email, @PasswordHash, @FullName, @CreatedAt)
            RETURNING id";

        var userId = Guid.NewGuid();

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var result = await connection.QueryFirstAsync<Guid>(
            sql,
            new
            {
                Id = userId,
                Email = email,
                PasswordHash = passwordHash,
                FullName = fullName,
                CreatedAt = DateTime.UtcNow
            });

        return result;
    }

    public async Task UpdateProfileAsync(Guid userId, string[] flavorTags, string[] dietaryRegime, string[] rejectedIngredients, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE user_profiles
            SET flavor_tags = @FlavorTags, dietary_regime = @DietaryRegime, rejected_ingredients = @RejectedIngredients, updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.ExecuteAsync(
            sql,
            new
            {
                UserId = userId,
                FlavorTags = flavorTags,
                DietaryRegime = dietaryRegime,
                RejectedIngredients = rejectedIngredients,
                UpdatedAt = DateTime.UtcNow
            });
    }
}
