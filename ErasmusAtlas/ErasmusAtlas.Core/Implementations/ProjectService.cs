using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.ViewModels.SharedViewModels;
using ErasmusAtlas.ViewModels.ProjectViewModels;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Core.Implementations;

public class ProjectService(
    IRepository<Project, Guid> projectRepository,
    IRepository<City, int> cityRepository,
    IRepository<ProjectType, int> projectTypeRepository,
    IRepository<Tag, int> tagRepository,
    IRepository<SavedProject, object> savedProjectRepository)
    : IProjectService
{
    public async Task<ProjectsIndexPageViewModel> GetAllFilteredAsync(ProjectFilterViewModel filter)
    {
        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        if (filter.PageSize < 1)
        {
            filter.PageSize = 12;
        }

        var query = projectRepository
            .GetAllAttached();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            string term = filter.SearchTerm.Trim();

            query = query.Where(p =>
                p.Title.Contains(term) ||
                p.Description.Contains(term));
        }

        if (filter.CityId.HasValue)
        {
            query = query.Where(p => p.CityId == filter.CityId.Value);
        }

        if (filter.ProjectTypeId.HasValue)
        {
            query = query.Where(p => p.ProjectTypeId == filter.ProjectTypeId.Value);
        }

        if (filter.TagIds is not null && filter.TagIds.Any())
        {
            query = query.Where(p => p.ProjectTags.Any(pt => filter.TagIds.Contains(pt.TagId)));
        }

        int totalCount = await query.CountAsync();

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(p => new ProjectInfoViewModel
            {
                Id = p.Id,
                Title = p.Title,
                DescriptionPreview = p.Description.Length > 220
                    ? p.Description.Substring(0, 220) + "..."
                    : p.Description,
                City = p.City != null ? p.City.Name : "Unknown",
                Institution = p.Institution != null ? p.Institution.Name : "Unknown",
                ProjectType = p.ProjectType != null ? p.ProjectType.Name : "Unknown",
                Tags = p.ProjectTags
                    .Select(pt => pt.Tag.Name)
                    .OrderBy(t => t)
                    .ToList(),
                CreatedOn = p.CreatedAt
            })
            .ToListAsync();

        return new ProjectsIndexPageViewModel
        {
            Filter = filter,
            Projects = projects,
            Cities = await GetCitiesAsync(),
            ProjectTypes = await GetProjectTypesAsync(),
            Tags = await GetTagsAsync(),
            CurrentPage = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProjectDetailsViewModel?> GetByIdAsync(Guid id, string userId)
    {
        var project = await projectRepository
            .GetAllAttached()
            .Where(p => p.Id == id)
            .Select(p => new ProjectDetailsViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                City = p.City != null ? p.City.Name : "Unknown",
                Institution = p.Institution != null ? p.Institution.Name : "Unknown",
                ProjectType = p.ProjectType.Name,
                Tags = p.ProjectTags
                    .Select(pt => pt.Tag.Name)
                    .OrderBy(t => t)
                    .ToList(),
                CreatedOn = p.CreatedAt,
                Latitude = p.Location != null ? p.Location.Y : null,
                Longitude = p.Location != null ? p.Location.X : null
            })
            .FirstOrDefaultAsync();

        if (project is null)
        {
            throw new NullReferenceException("Unable to find project!");
        }

        project.IsSaved = userId != null &&
                          await savedProjectRepository.GetAllAttached()
                                                      .AnyAsync(sp => sp.UserId == userId && sp.ProjectId == id);

        return project;
    }

    public async Task<IEnumerable<CityLookupViewModel>> GetCitiesAsync()
    {

        var cities = await cityRepository
            .GetAllAttached()
            .OrderBy(p => p.Name)
            .Select(c => new CityLookupViewModel
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();

        if (cities is null)
        {
            throw new NullReferenceException("Unable to find cities!");
        }

        return cities;
    }

    public async Task<IEnumerable<ProjectInfoViewModel>> GetLatestAsync(int count)
    {
        return await projectRepository
            .GetAllAttached()
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .Select(p => new ProjectInfoViewModel
            {
                Id = p.Id,
                Title = p.Title,
                DescriptionPreview = p.Description.Length > 220
                    ? p.Description.Substring(0, 220) + "..."
                    : p.Description,
                City = p.City != null ? p.City.Name : "Unknown",
                Institution = p.Institution != null ? p.Institution.Name : "Unknown",
                ProjectType = p.ProjectType.Name,
                Tags = p.ProjectTags
                    .Select(pt => pt.Tag.Name)
                    .OrderBy(t => t)
                    .ToList(),
                CreatedOn = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectTypeLookupViewModel>> GetProjectTypesAsync()
    {
        var projectTypes = await projectTypeRepository
            .GetAllAttached()
            .OrderBy(pt => pt.Name)
            .Select(pt => new ProjectTypeLookupViewModel
            {
                Id = pt.Id,
                Name = pt.Name
            })
            .ToListAsync();

        if (projectTypes is null)
        {
            throw new NullReferenceException("Unable to find project types!");
        }

        return projectTypes;
    }

    public async Task<IEnumerable<TagLookupViewModel>> GetTagsAsync()
    {
        var tags = await tagRepository
            .GetAllAttached()
            .OrderBy(pt => pt.Name)
            .Select(pt => new TagLookupViewModel
            {
                Id =pt.Id,
                Name = pt.Name
            })
            .ToListAsync();

        if (tags is null)
        {
            throw new NullReferenceException("Unable to find tags!");
        }

        return tags;
    }
}
