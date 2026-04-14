namespace ErasmusAtlas.ViewModels.PostViewModels;

public class PostIndexViewModel
{
    public ICollection<PostInfoViewModel> Posts { get; set; } = new List<PostInfoViewModel>();

    public string? City { get; set; }

    public string? Topic { get; set; }

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages
        => (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage
        => CurrentPage > 1;

    public bool HasNextPage
        => CurrentPage < TotalPages;
}
