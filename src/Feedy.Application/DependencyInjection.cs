namespace Feedy.Application;

using Feedy.Application.UseCases.GetRecipeFeed;
using Feedy.Application.UseCases.RegisterUser;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<FeedRankingService>();
        services.AddScoped<GetRecipeFeedService>();

        services.AddScoped<PasswordHasher>();
        services.AddScoped<RegisterUserService>();

        return services;
    }
}
