using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Infrastructure.Models;

[PrimaryKey(nameof(UserId), nameof(ProjectId))]
public class SavedProject
{
    public string UserId { get; set; } = null!;

    public ErasmusUser User { get; set; } = null!;

    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
