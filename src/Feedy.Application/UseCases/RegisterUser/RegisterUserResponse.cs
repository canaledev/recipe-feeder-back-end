namespace Feedy.Application.UseCases.RegisterUser;

/// <summary>
/// Response returned on successful user registration.
/// </summary>
public record RegisterUserResponse(string Token, Guid UserId, string Email, string FullName);
