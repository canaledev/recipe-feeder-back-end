using Feedy.Api.Endpoints;
using Feedy.Application.Interfaces;
using Feedy.Application.Services;
using Feedy.Application.UseCases.GetRecipeFeed;
using Feedy.Application.UseCases.RegisterUser;
using Feedy.Domain.Interfaces;
using Feedy.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Localization — .resx files under Resources/, IStringLocalizer<T> resolves by type namespace
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

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
builder.Services.AddScoped<ITranslationRepository>(sp => new TranslationRepository(connectionString ?? ""));

// Register application services
builder.Services.AddScoped<ITranslationService, TranslationService>();

// Register use case handlers (Application layer)
// GetRecipeFeed use case
builder.Services.AddScoped<GetRecipeFeedQueryHandler>();
builder.Services.AddScoped<FeedRankingService>();

// RegisterUser use case
builder.Services.AddScoped<RegisterUserCommandHandler>();
builder.Services.AddScoped<PasswordHasher>();

var app = builder.Build();

// Supported cultures: en (default), es, pt, fr, hi
// Unsupported codes (e.g. zh) fall back to en via DefaultRequestCulture
var supportedCultures = new[] { "en", "es", "pt", "fr", "hi" };
app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture("en")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
    // Reads Accept-Language header; unrecognised codes fall back to "en"
    options.FallBackToParentCultures = true;
    options.FallBackToParentUICultures = true;
});

// Middleware
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
