namespace Feedy.Domain;

using Feedy.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<DuplicateEmailChecker>();

        return services;
    }
}
