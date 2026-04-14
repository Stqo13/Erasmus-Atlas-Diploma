using ErasmusAtlas.ViewModels.SharedViewModels;

namespace ErasmusAtlas.ViewModels.ProjectViewModels;

public class ProjectsIndexPageViewModel
{
    public ProjectFilterViewModel Filter { get; set; } = new();

    public IEnumerable<ProjectInfoViewModel> Projects { get; set; } = new List<ProjectInfoViewModel>();

    public IEnumerable<CityLookupViewModel> Cities { get; set; } = new List<CityLookupViewModel>();

    public IEnumerable<ProjectTypeLookupViewModel> ProjectTypes { get; set; } = new List<ProjectTypeLookupViewModel>();

    public IEnumerable<TagLookupViewModel> Tags { get; set; } = new List<TagLookupViewModel>();

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages
        => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage
        => CurrentPage > 1;

    public bool HasNextPage
        => CurrentPage < TotalPages;
}
