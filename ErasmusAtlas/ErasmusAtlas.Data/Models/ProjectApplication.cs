using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ErasmusAtlas.Infrastructure.Models;

public class ProjectApplication
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public ErasmusUser User { get; set; } = null!;

    [Required]
    [MaxLength(24)]
    public string Status { get; set; } = "Pending";

    [MaxLength(3000)]
    public string? Motivation { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
