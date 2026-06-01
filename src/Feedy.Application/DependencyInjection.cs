namespace Feedy.Application;

using Feedy.Application.Configuration;
using Feedy.Application.UseCases.BrowsePlaylists;
using Feedy.Application.UseCases.GetPlaylistDetail;
using Feedy.Application.UseCases.GetRecipeFeed;
using Feedy.Application.UseCases.GetSubscribedPlaylists;
using Feedy.Application.UseCases.LoginUser;
using Feedy.Application.UseCases.Logout;
using Feedy.Application.UseCases.RefreshToken;
using Feedy.Application.UseCases.RegisterUser;
using Feedy.Application.UseCases.SubscribeToPlaylist;
using Feedy.Application.UseCases.UnsubscribeFromPlaylist;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddScoped<FeedRankingService>();
        services.AddScoped<GetRecipeFeedService>();

        // IPasswordHasher, IJwtTokenProvider, IRefreshTokenRepository registered in Feedy.Infrastructure
        services.AddScoped<RegisterUserService>();
        services.AddScoped<LoginUserService>();
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<LogoutService>();

        services.AddScoped<BrowsePlaylistsService>();
        services.AddScoped<GetSubscribedPlaylistsService>();
        services.AddScoped<GetPlaylistDetailService>();
        services.AddScoped<SubscribeToPlaylistService>();
        services.AddScoped<UnsubscribeFromPlaylistService>();

        return services;
    }
}
