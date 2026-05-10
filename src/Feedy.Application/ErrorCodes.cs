namespace Feedy.Application;

/// <summary>
/// Stable error code constants used in API error responses.
/// These keys must match the resource names in the .resx files.
/// </summary>
public static class ErrorCodes
{
    public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
    public const string InvalidEmail = "INVALID_EMAIL";
    public const string PasswordTooShort = "PASSWORD_TOO_SHORT";
}
