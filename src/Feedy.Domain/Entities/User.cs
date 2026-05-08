namespace Feedy.Domain.Entities;

/// <summary>
/// User domain entity. Immutable data container.
/// </summary>
public record User(
    Guid Id,
    string Email,
    string PasswordHash,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

/// <summary>
/// User profile with preferences. Immutable.
/// </summary>
public record UserProfile(
    Guid UserId,
    string[] FlavorTags,
    string[] DietaryRegime,
    string[] RejectedIngredients,
    DateTime UpdatedAt
);
