using Feedy.Api.Endpoints;
using Feedy.Application.UseCases.GetRecipeFeed;
using Feedy.Application.UseCases.RegisterUser;
using Feedy.Domain.Interfaces;
using Feedy.Infrastructure.Persistence;
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

// Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services
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

// Register repositories (Infrastructure layer)
builder.Services.AddScoped<IRecipeRepository>(sp => new RecipeRepository(connectionString ?? ""));
builder.Services.AddScoped<IUserRepository>(sp => new UserRepository(connectionString ?? ""));

// Register use case handlers (Application layer)
// GetRecipeFeed use case
builder.Services.AddScoped<GetRecipeFeedQueryHandler>();
builder.Services.AddScoped<FeedRankingService>();

// RegisterUser use case
builder.Services.AddScoped<RegisterUserCommandHandler>();
builder.Services.AddScoped<PasswordHasher>();

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
