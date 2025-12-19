using System.ComponentModel.DataAnnotations;

namespace ErasmusAtlas.Infrastructure.Models;

public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;
}
