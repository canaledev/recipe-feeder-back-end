namespace Feedy.Application.UseCases.RegisterUser;

using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;

/// <summary>
/// Orchestrates the RegisterUser use case.
/// Returns a Result — no exceptions for business rule violations.
/// </summary>
public class RegisterUserService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher _passwordHasher;

    public RegisterUserService(IUserRepository userRepository, PasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<RegisterUserResponse>> HandleAsync(
        RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
            return Result<RegisterUserResponse>.Fail(
                new Error("DUPLICATE_EMAIL", $"A user with email '{request.Email}' already exists."));

        var passwordHash = _passwordHasher.Hash(request.Password);

        var userId = await _userRepository.CreateAsync(
            request.Email,
            passwordHash,
            request.InitialFlavorTags,
            cancellationToken);

        return Result<RegisterUserResponse>.Ok(new RegisterUserResponse(userId, request.Email));
    }
}
