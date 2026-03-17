using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Infrastructure.Models;

[PrimaryKey(nameof(UserId), nameof(PostId))]
public class SavedPost
{
    public string UserId { get; set; } = null!;

    public ErasmusUser User { get; set; } = null!;

    public Guid PostId { get; set; }

    public Post Post { get; set; } = null!;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
