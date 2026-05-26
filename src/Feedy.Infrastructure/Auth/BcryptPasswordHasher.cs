namespace Feedy.Infrastructure.Auth;

using Feedy.Application.Interfaces;

/// <summary>
/// BCrypt implementation of IPasswordHasher. Work factor defaults to 11 (BCrypt.Net-Next default).
/// </summary>
internal sealed class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
