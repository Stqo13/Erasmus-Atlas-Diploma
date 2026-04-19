using ErasmusAtlas.ViewModels.ProjectViewModels;
using ErasmusAtlas.ViewModels.SharedViewModels;

namespace ErasmusAtlas.Core.Interfaces;

public interface IProjectService
{
    Task<ProjectsIndexPageViewModel> GetAllFilteredAsync(ProjectFilterViewModel filter);

    Task<ProjectDetailsViewModel?> GetByIdAsync(Guid id, string userId);

    Task<IEnumerable<CityLookupViewModel>> GetCitiesAsync();

    Task<IEnumerable<ProjectTypeLookupViewModel>> GetProjectTypesAsync();

    Task<IEnumerable<TagLookupViewModel>> GetTagsAsync();

    Task<IEnumerable<ProjectInfoViewModel>> GetLatestAsync(int count);

    Task CreateAsync(CreateProjectViewModel model, string userId);
}
