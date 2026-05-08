namespace Feedy.Application.UseCases.RegisterUser;

/// <summary>
/// Command to register a new user.
/// Input contract for the use case. Each use case has its own command/query.
/// </summary>
public class RegisterUserCommand
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string[] InitialFlavorTags { get; init; } = [];

    public RegisterUserCommand(string email, string password, string[] initialFlavorTags = default!)
    {
        Email = email;
        Password = password;
        InitialFlavorTags = initialFlavorTags ?? [];
    }
}
