using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Infrastructure.Models;

[PrimaryKey(nameof(PostId), nameof(TopicId))]
public class PostTopic
{
    public Guid PostId { get; set; }
    public virtual Post Post { get; set; } = null!;

    public int TopicId { get; set; }
    public virtual Topic Topic { get; set; } = null!;
}
