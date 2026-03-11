using System.ComponentModel.DataAnnotations;

namespace ErasmusAtlas.ViewModels.PostViewModels;

public class EditPostViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(160, MinimumLength = 5)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(8000, MinimumLength = 10)]
    public string Body { get; set; } = null!;

    public int? CityId { get; set; }

    [Required]
    public List<int> TopicIds { get; set; } = new();

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }
}
