namespace Feedy.Api.Endpoints;

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
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return Results.Created($"/api/users/{result.UserId}", result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
