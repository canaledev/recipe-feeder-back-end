namespace Feedy.Application.UseCases.RegisterUser;

using Feedy.Domain.Interfaces;

/// <summary>
/// Handler for RegisterUserCommand. Orchestrates the use case.
/// Does not reference any other use case's services or DTOs.
/// </summary>
public class RegisterUserCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserDto> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existing = await _userRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"User with email {command.Email} already exists");
        }

        // Validate password (min 8 chars, etc.)
        if (command.Password.Length < 8)
        {
            throw new InvalidOperationException("Password must be at least 8 characters");
        }

        // Hash password
        var passwordHash = _passwordHasher.Hash(command.Password);

        // Create user
        var userId = await _userRepository.CreateAsync(
            command.Email,
            passwordHash,
            command.InitialFlavorTags,
            cancellationToken);

        return new RegisterUserDto(userId, command.Email);
    }
}
