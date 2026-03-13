using ErasmusAtlas.ViewModels.SharedViewModels;

namespace ErasmusAtlas.ViewModels.ProjectViewModels;

public class ProjectsIndexPageViewModel
{
    public ProjectFilterViewModel Filter { get; set; } = new();

    public IEnumerable<ProjectInfoViewModel> Projects { get; set; } = new List<ProjectInfoViewModel>();

    public IEnumerable<CityLookupViewModel> Cities { get; set; } = new List<CityLookupViewModel>();

    public IEnumerable<ProjectTypeLookupViewModel> ProjectTypes { get; set; } = new List<ProjectTypeLookupViewModel>();

    public IEnumerable<TagLookupViewModel> Tags { get; set; } = new List<TagLookupViewModel>();
}
