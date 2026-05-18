namespace Feedy.Api.Controllers;

using Feedy.Application.UseCases.RegisterUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RegisterUserService _registerUserService;

    public UsersController(RegisterUserService registerUserService)
    {
        _registerUserService = registerUserService;
    }

    /// <summary>Register a new user account.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<RegisterUserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]    // FluentValidation failures
    [ProducesResponseType(StatusCodes.Status409Conflict)]     // Duplicate email
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _registerUserService.HandleAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Code switch
            {
                "DUPLICATE_EMAIL" => Conflict(new { result.Error.Code, result.Error.Message }),
                _ => UnprocessableEntity(new { result.Error.Code, result.Error.Message })
            };
        }

        return Created($"/api/users/{result.Value.UserId}", result.Value);
    }
}
