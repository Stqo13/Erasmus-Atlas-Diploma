using ErasmusAtlas.ViewModels.PostViewModels;
using ErasmusAtlas.ViewModels.ProjectViewModels;

namespace ErasmusAtlas.ViewModels.AdminViewModels;

public class AdminDashboardViewModel
{
    public int UsersCount { get; set; }
    public int PostsCount { get; set; }
    public int ProjectsCount { get; set; }
    public int SavedPostsCount { get; set; }
    public int SavedProjectsCount { get; set; }

    public IEnumerable<PostInfoViewModel> LatestPosts { get; set; } = new List<PostInfoViewModel>();
    public IEnumerable<ProjectInfoViewModel> LatestProjects { get; set; } = new List<ProjectInfoViewModel>();
    public IEnumerable<AdminUserListItemViewModel> LatestUsers { get; set; } = new List<AdminUserListItemViewModel>();
}
