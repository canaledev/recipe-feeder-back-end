namespace Feedy.Application.UseCases.LoginUser;

/// <summary>
/// Response returned on successful authentication.
/// RawRefreshToken is sent to the controller to set in an HttpOnly cookie — never serialized to JSON.
/// </summary>
public record LoginUserResponse(
    string AccessToken,
    string RawRefreshToken,
    DateTime RefreshTokenExpiresAt,
    Guid UserId,
    string Email,
    string FullName);
