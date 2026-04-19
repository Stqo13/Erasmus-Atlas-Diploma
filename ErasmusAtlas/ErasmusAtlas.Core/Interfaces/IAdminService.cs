using ErasmusAtlas.ViewModels.AdminViewModels;

namespace ErasmusAtlas.Core.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();

        Task<AdminUsersPageViewModel> GetUsersAsync();

        Task<AdminEditUserViewModel?> GetEditUserAsync(string userId);

        Task<bool> EditUserAsync(AdminEditUserViewModel model);

        Task<bool> DeleteUserAsync(string userId);

        Task<AdminPostsPageViewModel> GetPostsAsync();

        Task<AdminProjectsPageViewModel> GetProjectsAsync();

        Task<bool> DeletePostAsync(Guid postId);

        Task<bool> DeleteProjectAsync(Guid projectId);
    }
}
