using ErasmusAtlas.ViewModels.PostViewModels;

namespace ErasmusAtlas.ViewModels.AdminViewModels;

public class AdminPostsPageViewModel
{
    public IEnumerable<PostInfoViewModel> Posts { get; set; } 
        = new List<PostInfoViewModel>();
}
