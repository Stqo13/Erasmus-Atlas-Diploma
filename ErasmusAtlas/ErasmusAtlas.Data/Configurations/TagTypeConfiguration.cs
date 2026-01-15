using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ErasmusAtlas.Infrastructure.Models;

namespace ErasmusAtlas.Infrastructure.Configurations;

public class TagTypeConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();

        var tags = GenerateTags<Tag>("tags.json");

        builder.HasData(tags);
    }

    private static IEnumerable<Tag> GenerateTags<T>(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Data", fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Seed file not found: {path}");

        var json = File.ReadAllText(path);

        var items = JsonConvert.DeserializeObject<List<Tag>>(json)
            ?? throw new Exception($"Invalid json file: {fileName}");

        if (items.Any(x => x.Id <= 0))
            throw new Exception("HasData requires explicit non-zero Id values.");

        foreach (var t in items)
            t.Name = t.Name.Trim();

        items = items.OrderBy(x => x.Id).ToList();

        return items;
    }
}
