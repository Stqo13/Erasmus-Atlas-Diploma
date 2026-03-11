namespace ErasmusAtlas.ViewModels.PostViewModels;

public class DeletePostViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string City { get; set; } = null!;

    public List<string> Topics { get; set; } = new();
}
