using CaeriusNet.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fenrir.Framework.Extensions;

public static class ServiceCollectionExtension
{
    private static readonly ConfigurationBuilder ConfigurationBuilder = new();

    private static readonly string ConnectionString = ConfigurationBuilder
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile("appsettings.Development.json", true, true)
        .Build()
        .GetConnectionString("FenrirDb")!;

    public static IServiceCollection AddDependenciesInjection(this IServiceCollection services)
    {
        return services
            .RegisterCaeriusOrm(ConnectionString)
            .RegisterServices()
            .RegisterRepositories();
    }

    private static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        return services;
    }
}