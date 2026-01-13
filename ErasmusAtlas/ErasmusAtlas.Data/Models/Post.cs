using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NetTopologySuite.Geometries;

namespace ErasmusAtlas.Infrastructure.Models;

public class Post
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(160)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(8000)]
    public string Body { get; set; } = null!;

    [Required]
    [MaxLength(32)]
    public string Status { get; set; } = "Published";

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "geography")]
    public Point? Location { get; set; }

    public int? CityId { get; set; }
    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public ErasmusUser User { get; set; } = null!;
}
