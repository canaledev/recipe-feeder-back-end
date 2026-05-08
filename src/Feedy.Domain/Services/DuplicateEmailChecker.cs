namespace Feedy.Domain.Services;

using Feedy.Domain.Interfaces;
using Feedy.Domain.ValueObjects;

/// <summary>
/// Domain Service: Check if an email is already in use.
/// Spans the User aggregate: can't be in User itself (it has only one user).
/// Repository is injected at the boundary (API/Application), not here.
/// </summary>
public class DuplicateEmailChecker
{
    private readonly IUserRepository _userRepository;

    public DuplicateEmailChecker(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Check if email is unique. Throws if duplicate.
    /// </summary>
    public async Task EnsureEmailIsUniqueAsync(Email email, CancellationToken cancellationToken)
    {
        var existing = await _userRepository.GetByEmailAsync(email.Value, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Email {email.Value} is already registered");
        }
    }
}
