namespace Feedy.Application.Interfaces;

/// <summary>
/// Generates signed JWT access tokens. Implemented by Infrastructure.
/// </summary>
public interface IJwtTokenProvider
{
    string GenerateAccessToken(Guid userId, string email, string fullName);
}
