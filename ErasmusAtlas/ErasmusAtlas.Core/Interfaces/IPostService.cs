using ErasmusAtlas.ViewModels.PostViewModels;

namespace ErasmusAtlas.Core.Interfaces;

public interface IPostService
{
    Task<IEnumerable<PostInfoViewModel>> GetAllFilteredAsync(string? city, string? topic);
    Task<PostDetailsViewModel?> GetByIdAsync(Guid id);

    Task<CreatePostViewModel> GetCreateModelAsync();
    Task CreateAsync(CreatePostViewModel model, string userId);

    //Task<EditPostViewModel?> GetEditModelAsync(Guid id, string userId);
    //Task<bool> EditAsync(Guid id, EditPostViewModel model, string userId);

    //Task<DeletePostViewModel?> GetDeleteModelAsync(Guid id, string userId);
    //Task<bool> DeleteAsync(Guid id, string userId);

    //Task<IEnumerable<PostInfoViewModel>> GetMineAsync(string userId);
}
