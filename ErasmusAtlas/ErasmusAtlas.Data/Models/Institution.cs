using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErasmusAtlas.Infrastructure.Models;

public class Institution
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(180)]
    public string Name { get; set; } = null!;

    [MaxLength(400)]
    public string? WebsiteUrl { get; set; }

    public int? CityId { get; set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }
}
