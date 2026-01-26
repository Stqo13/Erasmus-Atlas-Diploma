namespace ErasmusAtlas.ViewModels.MapViewModels;

public class PostMapMarkerViewModel
{
    public double Lat { get; set; }
    public double Lng { get; set; }

    public List<PostMapMarkerPostViewModel> Posts { get; set; } = new();
}
