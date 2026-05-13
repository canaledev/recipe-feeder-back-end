using Feedy.Api.Endpoints;
using Feedy.Application;
using Feedy.Domain;
using Feedy.Infrastructure;
using Serilog;

// Bootstrap logger catches startup errors before host configuration is complete
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, config) => config
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId());

var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services
    .AddDomain()
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = jwtSettings["Authority"];
        options.Audience = jwtSettings["Audience"];
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? [])
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware
app.UseSerilogRequestLogging(); // Single structured event per HTTP request
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health")
    .AllowAnonymous();

app.MapRecipeEndpoints();
app.MapUserEndpoints();

app.Run();
