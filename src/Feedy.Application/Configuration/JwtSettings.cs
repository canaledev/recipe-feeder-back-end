namespace Feedy.Application.Configuration;

/// <summary>
/// Strongly-typed binding for JwtSettings in appsettings.json.
/// </summary>
public class JwtSettings
{
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}
