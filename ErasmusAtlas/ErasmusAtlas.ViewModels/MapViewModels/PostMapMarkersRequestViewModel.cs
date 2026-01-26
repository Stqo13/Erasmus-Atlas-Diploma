namespace ErasmusAtlas.ViewModels.MapViewModels;

public class PostMapMarkersRequestViewModel
{
    public string? BoundingBox { get; set; }
    public int? CityId { get; set; }
    public string? Topic { get; set; }
    public int CoordinateRoundingPrecision { get; set; } = 3;
    public int MaxPostsReturnedTotal { get; set; } = 1500;
    public int MaxPostsReturnedPerMarker { get; set; } = 25;
    public int PostBodyMaxLength { get; set; } = 0;
}
