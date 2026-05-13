namespace Feedy.Infrastructure;

using Feedy.Domain.Interfaces;
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

        return services;
    }
}
