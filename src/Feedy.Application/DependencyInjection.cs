namespace Feedy.Application;

using Feedy.Application.UseCases.GetRecipeFeed;
using Feedy.Application.UseCases.RegisterUser;
using Feedy.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<FeedRankingService>();
        services.AddScoped<GetRecipeFeedQueryHandler>(sp => new GetRecipeFeedQueryHandler(
            sp.GetRequiredService<IRecipeRepository>(),
            sp.GetRequiredService<IUserRepository>(),
            sp.GetRequiredService<FeedRankingService>()));

        services.AddScoped<PasswordHasher>();
        services.AddScoped<RegisterUserCommandHandler>(sp => new RegisterUserCommandHandler(
            sp.GetRequiredService<IUserRepository>(),
            sp.GetRequiredService<PasswordHasher>()));

        return services;
    }
}
