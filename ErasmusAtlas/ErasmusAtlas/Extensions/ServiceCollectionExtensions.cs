using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.Core.Implementations;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.Infrastructure.Repository;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;

namespace ErasmusAtlas.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterRepositories(
            this IServiceCollection services)
    {
        services.AddScoped<IRepository<Post, Guid>, Repository<Post, Guid>>();
        services.AddScoped<IRepository<City, int>, Repository<City, int>>();
        services.AddScoped<IRepository<Topic, int>, Repository<Topic, int>>();

        return services;
    }

    public static IServiceCollection RegisterUserDefinedServices(
        this IServiceCollection services)
    {
        services.AddScoped<IMapService, MapService>();
        services.AddScoped<IPostService, PostService>();

        return services;
    }
}
