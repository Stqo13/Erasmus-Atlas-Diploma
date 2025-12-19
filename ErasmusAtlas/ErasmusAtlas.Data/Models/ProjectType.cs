using System.ComponentModel.DataAnnotations;

namespace ErasmusAtlas.Infrastructure.Models;

public class ProjectType
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(60)]
    public string Name { get; set; } = null!;
}
