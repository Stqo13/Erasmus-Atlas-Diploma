using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NetTopologySuite.Geometries;

namespace ErasmusAtlas.Infrastructure.Models;

public class Project
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(12000)]
    public string Description { get; set; } = null!;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "geography")]
    public Point? Location { get; set; }

    [Required]
    public int ProjectTypeId { get; set; }

    [ForeignKey(nameof(ProjectTypeId))]
    public ProjectType ProjectType { get; set; } = null!;

    public int? InstitutionId { get; set; }

    [ForeignKey(nameof(InstitutionId))]
    public Institution? Institution { get; set; }

    public int? CityId { get; set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }
}
