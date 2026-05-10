namespace Feedy.Api.Endpoints;

using Feedy.Application;
using Feedy.Application.Interfaces;
using Feedy.Application.UseCases.RegisterUser;

/// <summary>
/// User-related endpoints.
/// </summary>
public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users")
            .WithName("Users");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithOpenApi()
            .AllowAnonymous()
            .Produces<RegisterUserDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> Register(
        RegisterUserCommand command,
        RegisterUserCommandHandler handler,
        ITranslationService translationService,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Created($"/api/users/{result.UserId}", result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            return Results.BadRequest(new
            {
                errorCode = ErrorCodes.UserAlreadyExists,
                message   = translationService.GetErrorMessage(ErrorCodes.UserAlreadyExists)
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("8 characters"))
        {
            return Results.BadRequest(new
            {
                errorCode = ErrorCodes.PasswordTooShort,
                message   = translationService.GetErrorMessage(ErrorCodes.PasswordTooShort)
            });
        }
    }
}
