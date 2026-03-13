using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using static ErasmusAtlas.Common.ApplicationConstraints.InstitutionConstraints;

namespace ErasmusAtlas.Infrastructure.Models;

public class Institution
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(InstitutionNameMaxLength)]
    public string Name { get; set; } = null!;

    [MaxLength(InstitutionWebsiteUrlMaxLength)]
    public string? WebsiteUrl { get; set; }

    public int? CityId { get; set; }

    [ForeignKey(nameof(CityId))]
    public City? City { get; set; }
}
