namespace Feedy.Application.UseCases.RegisterUser;

/// <summary>
/// Response returned on successful user registration.
/// </summary>
public record RegisterUserResponse(Guid UserId, string Email);
