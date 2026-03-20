using ErasmusAtlas.ViewModels.PostViewModels;
using ErasmusAtlas.ViewModels.ProjectViewModels;

namespace ErasmusAtlas.Core.Interfaces;

public interface ISavedContentService
{
    Task<bool> SavePostAsync(Guid postId, string userId);
    Task<bool> UnsavePostAsync(Guid postId, string userId);
    Task<bool> SaveProjectAsync(Guid projectId, string userId);
    Task<bool> UnsaveProjectAsync(Guid projectId, string userId);

    Task<bool> IsPostSavedAsync(Guid postId, string userId);
    Task<bool> IsProjectSavedAsync(Guid projectId, string userId);

    Task<IEnumerable<PostInfoViewModel>> GetSavedPostsAsync(string userId);
    Task<IEnumerable<ProjectInfoViewModel>> GetSavedProjectsAsync(string userId);
}
