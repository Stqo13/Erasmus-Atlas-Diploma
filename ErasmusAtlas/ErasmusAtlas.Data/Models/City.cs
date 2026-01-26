using System.ComponentModel.DataAnnotations;

namespace ErasmusAtlas.Infrastructure.Models;

public class City
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string CountryIso2 { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
