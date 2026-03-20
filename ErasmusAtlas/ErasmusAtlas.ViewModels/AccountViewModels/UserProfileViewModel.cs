using ErasmusAtlas.ViewModels.PostViewModels;
using ErasmusAtlas.ViewModels.ProjectViewModels;

namespace ErasmusAtlas.ViewModels.AccountViewModels;

public class UserProfileViewModel
{
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string? Bio { get; set; }

    public int PostsCount { get; set; }
    public int ApplicationsCount { get; set; }
    public int SavedPostsCount { get; set; }
    public int SavedProjectsCount { get; set; }

    public IEnumerable<PostInfoViewModel> RecentPosts { get; set; } = new List<PostInfoViewModel>();
    public IEnumerable<ProjectInfoViewModel> SavedProjectsPreview { get; set; } = new List<ProjectInfoViewModel>();

    public bool IsOwner { get; set; }
}
