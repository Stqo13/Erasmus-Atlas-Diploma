using ErasmusAtlas.ViewModels.MapViewModels;

namespace ErasmusAtlas.Core.Interfaces;

public interface IMapService
{
    Task<PostMapMarkersResponseViewModel> GetPostMapMarkersAsync(PostMapMarkersRequestViewModel request);
}
