using ErasmusAtlas.ViewModels.PostViewModels;
using ErasmusAtlas.ViewModels.SharedViewModels;

namespace ErasmusAtlas.Core.Interfaces;

public interface IPostService
{
    Task<PostIndexViewModel> GetAllFilteredAsync(string? city, string? topic, int page, int pageSize);
    Task<PostDetailsViewModel?> GetByIdAsync(Guid id, string? currentUserId);
    Task CreateAsync(CreatePostViewModel model, string userId);
    Task<EditPostViewModel?> GetEditModelAsync(Guid id, string userId);
    Task<bool> EditAsync(Guid id, EditPostViewModel model, string userId);
    Task<DeletePostViewModel?> GetDeleteModelAsync(Guid id, string userId);
    Task<bool> DeleteAsync(Guid id, string userId);
    Task<IEnumerable<PostInfoViewModel>> GetMineAsync(string userId);
    Task<IEnumerable<CityLookupViewModel>> GetCitiesAsync();
    Task<IEnumerable<TopicLookupViewModel>> GetTopicsAsync();
    Task<IEnumerable<PostInfoViewModel>> GetLatestAsync(int count);
}
