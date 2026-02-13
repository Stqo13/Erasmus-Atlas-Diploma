namespace ErasmusAtlas.ViewModels.PostViewModels
{
    public class PostDetailsViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Body { get; set; } = null!;

        public string Topic { get; set; } = null!;

        public string City { get; set; } = null!;

        public string AuthorName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
    }
}
