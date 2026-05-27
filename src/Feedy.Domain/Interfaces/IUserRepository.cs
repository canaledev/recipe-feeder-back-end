namespace Feedy.Domain.Interfaces;

using Feedy.Domain.Entities;

/// <summary>
/// Repository for User aggregate. Implemented by Infrastructure layer.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Get user by email. Returns null if not found.
    /// </summary>
    Task<UserData?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Get user profile with preferences.
    /// </summary>
    Task<UserProfileData?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a new user. Returns the newly created user ID.
    /// </summary>
    Task<Guid> CreateAsync(string email, string passwordHash, string fullName, CancellationToken cancellationToken);

    /// <summary>
    /// Update user profile preferences.
    /// </summary>
    Task UpdateProfileAsync(Guid userId, string[] flavorTags, string[] dietaryRegime, string[] rejectedIngredients, CancellationToken cancellationToken);
}
