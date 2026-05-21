using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using FluentAssertions;

namespace Feedy.Api.Tests.Cors;

public class CorsConfigurationTests
{
    private static readonly string[] AllowedOrigins =
    [
        "http://localhost:5173",
        "http://localhost:3000",
        "https://feedy.app"
    ];

    private static CorsPolicy BuildPolicy()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(AllowedOrigins
                .Select((origin, i) => new KeyValuePair<string, string?>($"CorsOrigins:{i}", origin)))
            .Build();

        var services = new ServiceCollection();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy
                    .WithOrigins(configuration.GetSection("CorsOrigins").Get<string[]>() ?? [])
                    .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                    .AllowAnyHeader();
            });
        });

        var provider = services.BuildServiceProvider();
        var corsOptions = provider.GetRequiredService<IOptions<CorsOptions>>();
        return corsOptions.Value.GetPolicy("AllowFrontend")!;
    }

    [Fact]
    public void Policy_AllowsLocalhostViteOrigin()
    {
        var policy = BuildPolicy();
        policy.Origins.Should().Contain("http://localhost:5173");
    }

    [Fact]
    public void Policy_AllowsLocalhostReactOrigin()
    {
        var policy = BuildPolicy();
        policy.Origins.Should().Contain("http://localhost:3000");
    }

    [Fact]
    public void Policy_AllowsProductionOrigin()
    {
        var policy = BuildPolicy();
        policy.Origins.Should().Contain("https://feedy.app");
    }

    [Fact]
    public void Policy_AllowsGetMethod()
    {
        var policy = BuildPolicy();
        policy.Methods.Should().Contain("GET");
    }

    [Fact]
    public void Policy_AllowsPostMethod()
    {
        var policy = BuildPolicy();
        policy.Methods.Should().Contain("POST");
    }

    [Fact]
    public void Policy_AllowsPutMethod()
    {
        var policy = BuildPolicy();
        policy.Methods.Should().Contain("PUT");
    }

    [Fact]
    public void Policy_AllowsDeleteMethod()
    {
        var policy = BuildPolicy();
        policy.Methods.Should().Contain("DELETE");
    }

    [Fact]
    public void Policy_AllowsPatchMethod()
    {
        var policy = BuildPolicy();
        policy.Methods.Should().Contain("PATCH");
    }

    [Fact]
    public void Policy_DoesNotAllowArbitraryMethod()
    {
        var policy = BuildPolicy();
        // AllowAnyMethod would set IsAnyMethodAllowed = true; WithMethods sets explicit list
        policy.AllowAnyMethod.Should().BeFalse();
    }

    [Fact]
    public void Policy_AllowsAnyHeader()
    {
        var policy = BuildPolicy();
        policy.AllowAnyHeader.Should().BeTrue();
    }
}
