using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Infrastructure.Models;

[PrimaryKey(nameof(TagId), nameof(ProjectId))]
public class ProjectTag
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}
