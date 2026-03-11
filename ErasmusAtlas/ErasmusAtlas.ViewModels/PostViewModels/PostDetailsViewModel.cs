namespace ErasmusAtlas.ViewModels.PostViewModels
{
    public class PostDetailsViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string Body { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public string City { get; set; } = null!;

        public string AuthorId { get; set; } = null!;

        public string AuthorName { get; set; } = null!;

        public List<string> Topics { get; set; } = new();

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public bool CanEdit { get; set; }
    }
}
