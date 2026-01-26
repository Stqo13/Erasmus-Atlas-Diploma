namespace ErasmusAtlas.ViewModels.MapViewModels;

public class PostMapMarkerPostViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Body { get; set; }
    public List<string> Topics { get; set; } = new();
    public string CreatedAt { get; set; } = null!;
}
