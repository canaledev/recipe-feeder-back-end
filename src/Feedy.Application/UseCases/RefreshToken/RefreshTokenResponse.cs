namespace Feedy.Application.UseCases.RefreshToken;

/// <summary>
/// Response for the RefreshToken use case.
/// RawRefreshToken is passed to the controller to set in an HttpOnly cookie — never serialized to JSON.
/// </summary>
public record RefreshTokenResponse(
    string AccessToken,
    string RawRefreshToken,
    DateTime RefreshTokenExpiresAt);
