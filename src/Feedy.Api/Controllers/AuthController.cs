namespace Feedy.Api.Controllers;

using Feedy.Application.UseCases.LoginUser;
using Feedy.Application.UseCases.RegisterUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserService _registerService;
    private readonly LoginUserService _loginService;

    public AuthController(RegisterUserService registerService, LoginUserService loginService)
    {
        _registerService = registerService;
        _loginService = loginService;
    }

    /// <summary>Register a new user account and receive a JWT.</summary>
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
        return Created(
            $"/api/auth/me",
            new { v.Token, User = new { Id = v.UserId, v.Email, v.FullName } });
    }

    /// <summary>Authenticate with email and password and receive a JWT.</summary>
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
        return Ok(new { v.Token, User = new { Id = v.UserId, v.Email, v.FullName } });
    }
}
