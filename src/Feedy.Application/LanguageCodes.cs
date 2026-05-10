namespace Feedy.Application;

/// <summary>
/// BCP-47 language code constants.
/// String constants instead of an enum to avoid integer-to-string mapping
/// and keep code self-documenting (language_code == LanguageCodes.Spanish reads clearly).
/// Enabled languages are controlled by the languages table (is_enabled flag);
/// this class only provides named constants to prevent magic strings in C# code.
/// </summary>
public static class LanguageCodes
{
    public const string English    = "en";
    public const string Spanish    = "es";
    public const string Portuguese = "pt";
    public const string French     = "fr";
    public const string Hindi      = "hi";

    /// <summary>
    /// The fallback language used when no translation is found.
    /// Matches the DEFAULT on the languages.is_enabled column.
    /// </summary>
    public const string Default = English;

    /// <summary>
    /// All cultures actively served by the backend.
    /// Used to configure RequestLocalizationMiddleware.
    /// </summary>
    public static readonly IReadOnlyList<string> Supported = [English, Spanish, Portuguese, French, Hindi];
}
