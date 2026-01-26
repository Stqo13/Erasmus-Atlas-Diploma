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

        return services;
    }

    public static IServiceCollection RegisterUserDefinedServices(
        this IServiceCollection services)
    {
        services.AddScoped<IMapService, MapService>();

        return services;
    }
}
