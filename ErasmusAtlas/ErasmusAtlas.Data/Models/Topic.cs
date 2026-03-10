using System.ComponentModel.DataAnnotations;

namespace ErasmusAtlas.Infrastructure.Models;

public class Topic
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    public ICollection<PostTopic> PostTopics { get; set; } = new List<PostTopic>();
}
