using System.ComponentModel.DataAnnotations;

using static ErasmusAtlas.Common.ApplicationConstraints.CityConstraints;

namespace ErasmusAtlas.Infrastructure.Models;

public class City
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(CityNameMaxLength)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(CountryIsoLength, MinimumLength = CountryIsoLength)]
    public string CountryIso2 { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
