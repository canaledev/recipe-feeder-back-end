namespace Feedy.Api.Controllers;

using System.Security.Claims;
using Feedy.Application.UseCases.LoginUser;
using Feedy.Application.UseCases.Logout;
using Feedy.Application.UseCases.RefreshToken;
using Feedy.Application.UseCases.RegisterUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "refresh_token";

    private readonly RegisterUserService _registerService;
    private readonly LoginUserService _loginService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly LogoutService _logoutService;

    public AuthController(
        RegisterUserService registerService,
        LoginUserService loginService,
        RefreshTokenService refreshTokenService,
        LogoutService logoutService)
    {
        _registerService = registerService;
        _loginService = loginService;
        _refreshTokenService = refreshTokenService;
        _logoutService = logoutService;
    }

    /// <summary>Register a new user account and receive an access token.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _registerService.HandleAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "DUPLICATE_EMAIL" => Conflict(new { result.Error.Code, result.Error.Message }),
                _ => UnprocessableEntity(new { result.Error.Code, result.Error.Message })
            };
        }

        var v = result.Value;
        SetRefreshTokenCookie(v.RawRefreshToken, v.RefreshTokenExpiresAt);
        return Created(
            "/api/auth/me",
            new { v.AccessToken, User = new { Id = v.UserId, v.Email, v.FullName } });
    }

    /// <summary>Authenticate with email and password and receive an access token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _loginService.HandleAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "INVALID_CREDENTIALS" => Unauthorized(new { result.Error.Code, result.Error.Message }),
                _ => UnprocessableEntity(new { result.Error.Code, result.Error.Message })
            };
        }

        var v = result.Value;
        SetRefreshTokenCookie(v.RawRefreshToken, v.RefreshTokenExpiresAt);
        return Ok(new { v.AccessToken, User = new { Id = v.UserId, v.Email, v.FullName } });
    }

    /// <summary>
    /// Use the refresh_token cookie to obtain a new access token and rotate the refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken = default)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName];
        var result = await _refreshTokenService.HandleAsync(rawToken, cancellationToken);

        if (!result.IsSuccess)
            return Unauthorized(new { result.Error.Code, result.Error.Message });

        var v = result.Value;
        SetRefreshTokenCookie(v.RawRefreshToken, v.RefreshTokenExpiresAt);
        return Ok(new { v.AccessToken });
    }

    /// <summary>
    /// Revoke all refresh tokens for the authenticated user and clear the cookie.
    /// Requires Authorization: Bearer &lt;accessToken&gt; header.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        await _logoutService.HandleAsync(userId, cancellationToken);

        // Clear the cookie by setting an expired date
        Response.Cookies.Append(RefreshTokenCookieName, string.Empty, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });

        return NoContent();
    }

    private void SetRefreshTokenCookie(string rawToken, DateTime expiresAt)
    {
        Response.Cookies.Append(RefreshTokenCookieName, rawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = new DateTimeOffset(expiresAt, TimeSpan.Zero)
        });
    }
}
