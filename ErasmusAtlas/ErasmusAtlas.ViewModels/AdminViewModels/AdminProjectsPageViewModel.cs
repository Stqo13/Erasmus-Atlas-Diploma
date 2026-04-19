using ErasmusAtlas.ViewModels.ProjectViewModels;

namespace ErasmusAtlas.ViewModels.AdminViewModels;

public class AdminProjectsPageViewModel
{
    public IEnumerable<ProjectInfoViewModel> Projects { get; set; } 
        = new List<ProjectInfoViewModel>();
}
