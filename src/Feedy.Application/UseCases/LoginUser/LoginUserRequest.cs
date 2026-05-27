namespace Feedy.Application.UseCases.LoginUser;

/// <summary>
/// Input for the LoginUser use case. Validated at the presentation layer before reaching the service.
/// </summary>
public record LoginUserRequest(string Email, string Password);
