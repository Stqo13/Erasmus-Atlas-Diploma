using ErasmusAtlas.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace ErasmusAtlas.Infrastructure.Configurations;

public class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        var topics = GenerateTopics<Topic>("topics.json");

        builder.HasData(topics);
    }

    private static IEnumerable<Topic> GenerateTopics<T>(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Data", fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Seed file not found: {path}");

        var json = File.ReadAllText(path);

        var items = JsonConvert.DeserializeObject<List<Topic>>(json)
            ?? throw new Exception($"Invalid json file: {fileName}");

        if (items.Any(x => x.Id <= 0))
            throw new Exception("HasData requires explicit non-zero Id values.");

        foreach (var t in items)
            t.Name = t.Name.Trim();

        items = items.OrderBy(x => x.Id).ToList();

        return items;
    }
}
