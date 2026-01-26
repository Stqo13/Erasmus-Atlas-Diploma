using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json; 

using ErasmusAtlas.Infrastructure.Models;


namespace ErasmusAtlas.Infrastructure.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasData(GenerateCities());
    }

    private static IEnumerable<City> GenerateCities()
    {
        var baseDir = AppContext.BaseDirectory;
        var path = Path.Combine(baseDir, "Data", "cities.json");

        if (!File.Exists(path))
            throw new FileNotFoundException($"Seed file not found: {path}");

        var json = File.ReadAllText(path);

        var cities = JsonConvert.DeserializeObject<List<City>>(json)
            ?? throw new Exception("Invalid json path or content for cities.json");

        if (cities.Any(c => c.Id <= 0))
            throw new Exception("HasData requires explicit non-zero Id values for City.");

        return cities;
    }
}
