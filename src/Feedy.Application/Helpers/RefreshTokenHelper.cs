namespace Feedy.Application.Helpers;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Shared helpers for opaque refresh token generation and hashing.
/// Refresh tokens are 64 random bytes encoded as Base64 (not JWTs).
/// </summary>
internal static class RefreshTokenHelper
{
    /// <summary>
    /// Generates a cryptographically random opaque token and its SHA-256 hex digest.
    /// Store only the hash in DB — never the raw token.
    /// </summary>
    public static (string Raw, string Hash) GenerateTokenPair()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return (raw, HashToken(raw));
    }

    /// <summary>
    /// Computes the SHA-256 hex digest of a raw token string.
    /// Used to look up tokens from DB without storing the raw value.
    /// </summary>
    public static string HashToken(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
