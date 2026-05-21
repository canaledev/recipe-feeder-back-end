namespace Feedy.Api.Middleware;

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

/// <summary>
/// Catches any unhandled exception that escapes the application pipeline,
/// logs it with full stack trace, and returns a RFC 7807 ProblemDetails 500 response.
/// The exception detail is included only in the Development environment.
/// </summary>
public class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger,
    IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Title = "An unexpected error occurred.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = env.IsDevelopment() ? ex.Message : null
            };

            await JsonSerializer.SerializeAsync(
                context.Response.Body,
                problem,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
