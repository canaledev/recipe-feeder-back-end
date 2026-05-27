namespace Feedy.Application.UseCases.RegisterUser;

using Feedy.Application.Interfaces;
using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;

/// <summary>
/// Orchestrates the RegisterUser use case.
/// Returns a Result — no exceptions for business rule violations.
/// </summary>
public class RegisterUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    public RegisterUserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenProvider jwtTokenProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenProvider = jwtTokenProvider;
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
            request.FullName,
            cancellationToken);

        var token = _jwtTokenProvider.GenerateToken(userId, request.Email, request.FullName);

        return Result<RegisterUserResponse>.Ok(
            new RegisterUserResponse(token, userId, request.Email, request.FullName));
    }
}
