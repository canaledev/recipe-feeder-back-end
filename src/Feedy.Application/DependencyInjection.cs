namespace Feedy.Application;

using Feedy.Application.UseCases.GetRecipeFeed;
using Feedy.Application.UseCases.LoginUser;
using Feedy.Application.UseCases.RegisterUser;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<FeedRankingService>();
        services.AddScoped<GetRecipeFeedService>();

        // IPasswordHasher and IJwtTokenProvider are registered in Feedy.Infrastructure
        services.AddScoped<RegisterUserService>();
        services.AddScoped<LoginUserService>();

        return services;
    }
}
