using System.ComponentModel.DataAnnotations;

using static ErasmusAtlas.Common.ApplicationConstraints.TagConstraints;

namespace ErasmusAtlas.Infrastructure.Models;

public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(TagNameMaxLength)]
    public string Name { get; set; } = null!;
}
