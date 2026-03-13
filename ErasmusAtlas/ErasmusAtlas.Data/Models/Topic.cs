using System.ComponentModel.DataAnnotations;

using static ErasmusAtlas.Common.ApplicationConstraints.TopicConstraints;

namespace ErasmusAtlas.Infrastructure.Models;

public class Topic
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(TopicNameMaxLength)]
    public string Name { get; set; } = null!;

    public ICollection<PostTopic> PostTopics { get; set; } = new List<PostTopic>();
}
