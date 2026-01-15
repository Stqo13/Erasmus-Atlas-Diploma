using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ErasmusAtlas.Infrastructure.Models;

namespace ErasmusAtlas.Infrastructure.Configurations;

public class ProjectTypeConfiguration : IEntityTypeConfiguration<ProjectType>
{
    public void Configure(EntityTypeBuilder<ProjectType> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();

        var projectTypes = GenerateProjectTypes<ProjectType>("projectTypes.json");

        builder.HasData(projectTypes);
    }

    private static IEnumerable<ProjectType> GenerateProjectTypes<T>(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Data", fileName);

        if (!File.Exists(path))
            throw new FileNotFoundException($"Seed file not found: {path}");

        var json = File.ReadAllText(path);

        var items = JsonConvert.DeserializeObject<List<ProjectType>>(json)
            ?? throw new Exception($"Invalid json file: {fileName}");

        if (items.Any(x => x.Id <= 0))
            throw new Exception("HasData requires explicit non-zero Id values.");

        foreach (var p in items)
            p.Name = p.Name.Trim();

        items = items.OrderBy(x => x.Id).ToList();

        return items;
    }
}
