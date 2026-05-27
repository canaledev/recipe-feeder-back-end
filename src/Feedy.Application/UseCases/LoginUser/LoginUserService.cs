namespace Feedy.Application.UseCases.LoginUser;

using Feedy.Application.Interfaces;
using Feedy.Domain.Common;
using Feedy.Domain.Interfaces;

/// <summary>
/// Authenticates a user and issues a JWT.
/// Both "email not found" and "wrong password" return the same error to prevent account enumeration.
/// </summary>
public class LoginUserService
{
    // Static instance ensures identical Code and Message regardless of failure branch.
    private static readonly Error InvalidCredentials =
        new("INVALID_CREDENTIALS", "Invalid email or password.");

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    public LoginUserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenProvider jwtTokenProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenProvider = jwtTokenProvider;
    }

    public async Task<Result<LoginUserResponse>> HandleAsync(
        LoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Same branch for unknown email and wrong password — anti-enumeration.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginUserResponse>.Fail(InvalidCredentials);

        var token = _jwtTokenProvider.GenerateToken(user.Id, user.Email, user.FullName);

        return Result<LoginUserResponse>.Ok(
            new LoginUserResponse(token, user.Id, user.Email, user.FullName));
    }
}
