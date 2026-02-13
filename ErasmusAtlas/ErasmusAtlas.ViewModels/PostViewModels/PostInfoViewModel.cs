namespace ErasmusAtlas.ViewModels.PostViewModels;

public class PostInfoViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Topic { get; set; } = null!;
    public DateTime CreatedOn { get; set; }
}
