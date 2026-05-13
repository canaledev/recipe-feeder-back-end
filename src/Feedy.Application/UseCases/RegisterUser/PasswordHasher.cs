namespace Feedy.Application.UseCases.RegisterUser;

/// <summary>
/// Password hashing service. Use case specific.
/// Implements a simple bcrypt-like pattern (would use BCrypt.Net-Next in production).
/// </summary>
internal class PasswordHasher
{
    public string Hash(string password)
    {
        // TODO: Use BCrypt.Net-Next for production
        // For now, return a placeholder
        return $"hashed_{password}";
    }

    public bool Verify(string password, string hash)
    {
        // TODO: Use BCrypt.Net-Next for production
        // For now, do a simple comparison
        return hash == $"hashed_{password}";
    }
}
