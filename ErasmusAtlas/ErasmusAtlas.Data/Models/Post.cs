using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static ErasmusAtlas.Common.ApplicationConstraints.PostConstraints;

using NetTopologySuite.Geometries;

namespace ErasmusAtlas.Infrastructure.Models;

public class Post
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(PostTitleMaxLength)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(PostBodyMaxLength)]
    public string Body { get; set; } = null!;

    [Required]
    [MaxLength(PostStatusMaxLength)]
    public string Status { get; set; } = "Published";

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "geography")]
    public Point? Location { get; set; }

    public int? CityId { get; set; }
    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }

    public ICollection<SavedPost> SavedByUsers { get; set; }
        = new List<SavedPost>();

    [Required]
    public string UserId { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public ErasmusUser User { get; set; } = null!;

    public ICollection<PostTopic> PostTopics { get; set; }
        = new List<PostTopic>();
}
