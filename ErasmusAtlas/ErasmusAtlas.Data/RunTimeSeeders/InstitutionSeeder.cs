using ErasmusAtlas.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Infrastructure.RunTimeSeeders;

public static class InstitutionSeeder
{
    public static async Task SeedAsync(ErasmusAtlasDbContext db)
    {
        if (await db.Institutions.AnyAsync())
            return;

        var cities = await db.Cities.OrderBy(c => c.Id).ToListAsync();
        if (cities.Count == 0)
            throw new Exception("No cities found. Seed cities first.");

        var institutions = new List<Institution>();

        foreach (var c in cities)
        {
            institutions.Add(new Institution
            {
                Name = $"University of {c.Name}",
                WebsiteUrl = null,
                CityId = c.Id
            });

            institutions.Add(new Institution
            {
                Name = $"{c.Name} Institute of Technology",
                WebsiteUrl = null,
                CityId = c.Id
            });
        }

        await db.Institutions.AddRangeAsync(institutions);
        await db.SaveChangesAsync();
    }
}
