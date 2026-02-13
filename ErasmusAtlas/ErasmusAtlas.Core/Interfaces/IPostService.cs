using ErasmusAtlas.ViewModels.PostViewModels;

namespace ErasmusAtlas.Core.Interfaces;

public interface IPostService
{
    Task<IEnumerable<PostInfoViewModel>> GetAllFilteredAsync(string? city, string? topic);
    Task<PostDetailsViewModel> GetByIdAsync(Guid id);
}
