using ErasmusAtlas.ViewModels.AccountViewModels;

namespace ErasmusAtlas.Core.Interfaces;

public interface IProfileService
{
    Task<UserProfileViewModel?> GetProfileAsync(string userId, string? currentUserId = null);
    Task<EditProfileViewModel?> GetEditModelAsync(string userId);
    Task<bool> EditAsync(string userId, EditProfileViewModel model);
}
