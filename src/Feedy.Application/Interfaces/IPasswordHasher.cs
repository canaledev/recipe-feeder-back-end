namespace Feedy.Application.Interfaces;

/// <summary>
/// Abstracts password hashing so the Application layer never depends on a specific algorithm.
/// Implemented by Infrastructure (BCrypt).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
