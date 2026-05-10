namespace Feedy.Domain.Entities;

/// <summary>
/// Data contract for recipe from repository.
/// </summary>
public record RecipeData(
    Guid Id,
    string Title,
    string Description,
    string[] Tags,
    string Difficulty,
    int PrepTimeMinutes,
    string MainImageUrl,
    string SourceLanguage
);

/// <summary>
/// Data contract for user profile from repository.
/// </summary>
public record UserProfileData(
    Guid UserId,
    string[] FlavorTags,
    string[] DietaryRegime,
    string[] RejectedIngredients
);

/// <summary>
/// Data contract for user from repository.
/// </summary>
public record UserData(
    Guid Id,
    string Email,
    string PasswordHash
);
