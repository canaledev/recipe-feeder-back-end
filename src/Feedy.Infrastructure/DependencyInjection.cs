namespace Feedy.Infrastructure;

using Feedy.Application.Interfaces;
using Feedy.Domain.Interfaces;
using Feedy.Infrastructure.Auth;
using Feedy.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

        services.AddScoped<IRecipeRepository>(_ => new RecipeRepository(connectionString));
        services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
        services.AddScoped<IRefreshTokenRepository>(_ => new RefreshTokenRepository(connectionString));
        services.AddScoped<IPlaylistRepository>(_ => new PlaylistRepository(connectionString));
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

        // DB reachability check — verifies the PostgreSQL connection is alive
        services.AddHealthChecks()
            .AddNpgSql(connectionString, name: "postgres", tags: ["db", "ready"]);

        return services;
    }
}
