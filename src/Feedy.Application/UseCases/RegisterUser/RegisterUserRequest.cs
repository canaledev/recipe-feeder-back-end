namespace Feedy.Application.UseCases.RegisterUser;

/// <summary>
/// Input for the RegisterUser use case. Validated at the presentation layer before reaching the service.
/// </summary>
public record RegisterUserRequest(
    string Email,
    string Password,
    string FullName);
