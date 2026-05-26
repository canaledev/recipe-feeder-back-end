namespace Feedy.Application.UseCases.LoginUser;

/// <summary>
/// Response returned on successful authentication.
/// </summary>
public record LoginUserResponse(string Token, Guid UserId, string Email, string FullName);
