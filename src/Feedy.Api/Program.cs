using Feedy.Application;
using Feedy.Domain;
using Feedy.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using System.Text.Json;

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

// Controllers with camelCase JSON to match the TypeScript frontend contract
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

// FluentValidation — auto-validates requests before controller actions run,
// returns 400 ValidationProblemDetails on failure (no try-catch needed in controllers)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// DisableDataAnnotationsValidation = true ensures only FluentValidation messages appear,
// preventing duplicate errors from the ASP.NET model binder on non-nullable properties.
builder.Services.AddFluentValidationAutoValidation(config =>
    config.DisableDataAnnotationsValidation = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// Health check kept as minimal API — no auth, no validation needed
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("Health")
    .AllowAnonymous();

app.MapControllers();

app.Run();
