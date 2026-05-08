namespace Feedy.Application.UseCases.RegisterUser;

/// <summary>
/// Response DTO for RegisterUser. Use case specific.
/// NOT shared with other use cases.
/// </summary>
public class RegisterUserDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;

    public RegisterUserDto(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}
