using System.Text;
using System.Text.Json;
using Feedy.Api.Middleware;
using Feedy.Application;
using Feedy.Application.Interfaces;
using Feedy.Application.Services;
using Feedy.Domain;
using Feedy.Domain.Interfaces;
using Feedy.Infrastructure;
using Feedy.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
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
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Localization — .resx files under Resources/, IStringLocalizer<T> resolves by type namespace
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services
    .AddDomain()
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration);

// Translation infrastructure (i18n) — registered separately until AddInfrastructure absorbs them
builder.Services.AddScoped<ITranslationRepository>(sp => new TranslationRepository(connectionString ?? ""));
builder.Services.AddScoped<ITranslationService, TranslationService>();

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
builder.Services.AddSwaggerGen(options =>
{
    // Include XML doc comments so controller <summary> tags appear in Swagger UI
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Secret"]!)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? [])
            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
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

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseSerilogRequestLogging();

// Localization middleware — reads Accept-Language header; unsupported codes fall back to "en"
var supportedCultures = LanguageCodes.Supported.ToArray();
app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture(LanguageCodes.Default)
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
    options.FallBackToParentCultures = true;
    options.FallBackToParentUICultures = true;
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(result);
    }
}).AllowAnonymous();

app.MapControllers();

app.Run();
