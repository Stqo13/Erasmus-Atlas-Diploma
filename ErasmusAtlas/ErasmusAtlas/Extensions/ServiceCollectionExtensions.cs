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
        services.AddScoped<IRepository<Project, Guid>, Repository<Project, Guid>>();
        services.AddScoped<IRepository<City, int>, Repository<City, int>>();
        services.AddScoped<IRepository<Topic, int>, Repository<Topic, int>>();
        services.AddScoped<IRepository<ProjectType, int>, Repository<ProjectType, int>>();
        services.AddScoped<IRepository<Tag, int>, Repository<Tag, int>>();
        services.AddScoped<IRepository<Institution, int>, Repository<Institution, int>>();
        services.AddScoped<IRepository<SavedPost, object>, Repository<SavedPost, object>>();
        services.AddScoped<IRepository<SavedProject, object>, Repository<SavedProject, object>>();

        return services;
    }

    public static IServiceCollection RegisterUserDefinedServices(
        this IServiceCollection services)
    {
        services.AddScoped<IMapService, MapService>();
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ISavedContentService, SavedContentService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
