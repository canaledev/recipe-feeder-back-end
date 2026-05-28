namespace Feedy.Application.UseCases.RegisterUser;

/// <summary>
/// Response returned on successful user registration.
/// RawRefreshToken is sent to the controller to set in an HttpOnly cookie — never serialized to JSON.
/// </summary>
public record RegisterUserResponse(
    string AccessToken,
    string RawRefreshToken,
    DateTime RefreshTokenExpiresAt,
    Guid UserId,
    string Email,
    string FullName);
