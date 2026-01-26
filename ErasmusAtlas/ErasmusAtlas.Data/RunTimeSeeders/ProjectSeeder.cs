using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

using ErasmusAtlas.Infrastructure.Models;

namespace ErasmusAtlas.Infrastructure.RunTimeSeeders;

public static class ProjectSeeder
{
    private sealed class Mulberry32
    {
        private uint _state;
        public Mulberry32(uint seed) => _state = seed;

        public double NextDouble()
        {
            _state += 0x6D2B79F5;
            uint t = _state;
            t = unchecked(t ^ (t >> 15));
            t = unchecked(t * (t | 1));
            t ^= t + unchecked(t * (t ^ (t >> 7)));
            t ^= t >> 14;
            return (t & 0xFFFFFFFFu) / 4294967296.0;
        }

        public int NextInt(int maxExclusive) => (int)(NextDouble() * maxExclusive);
    }

    private static readonly string[] TitleTemplates =
    [
        "Erasmus {0} Program in {1}",
        "{1} {0} Opportunity for Students",
        "{0} Placement – {1}",
        "{0} Track at {2} in {1}",
        "{0} Exchange Experience in {1}",
    ];

    private static readonly string[] DescriptionParagraphs =
    [
        "This opportunity is designed for motivated students who want to gain international experience and practical skills.",
        "You’ll join a small cohort, work with supportive mentors, and participate in structured activities across the semester.",
        "The program focuses on real-world outcomes: teamwork, communication, and producing a tangible result by the end.",
        "Expect a mix of independent work and group sessions. Attendance and participation matter, but the environment is friendly.",
        "Accommodation and living costs vary by city; plan your budget early and apply as soon as you’re ready.",
    ];

    public static async Task SeedAsync(ErasmusAtlasDbContext db, int totalProjects = 120)
    {
        if (await db.Projects.AnyAsync())
            return;

        var rng = new Mulberry32(6060);

        var cities = await db.Cities.OrderBy(c => c.Id).ToListAsync();
        var types = await db.ProjectTypes.OrderBy(t => t.Id).ToListAsync();
        var tags = await db.Tags.OrderBy(t => t.Id).ToListAsync();
        var institutions = await db.Institutions.OrderBy(i => i.Id).ToListAsync();

        if (cities.Count == 0) throw new Exception("No cities found.");
        if (types.Count == 0) throw new Exception("No project types found.");
        if (tags.Count == 0) throw new Exception("No tags found.");
        if (institutions.Count == 0) throw new Exception("No institutions found.");

        var projects = new List<Project>(totalProjects);
        var projectTags = new List<ProjectTag>(totalProjects * 2);

        for (int i = 0; i < totalProjects; i++)
        {
            var city = cities[i % cities.Count];
            var type = types[i % types.Count];

            var instInCity = institutions.Where(x => x.CityId == city.Id).ToList();
            Institution? inst = instInCity.Count > 0
                ? instInCity[rng.NextInt(instInCity.Count)]
                : institutions[rng.NextInt(institutions.Count)];

            var titleTemplate = TitleTemplates[rng.NextInt(TitleTemplates.Length)];
            var title = string.Format(titleTemplate, type.Name, city.Name, inst.Name);

            var paragraphsCount = 2 + rng.NextInt(3);
            var descParts = new List<string>(paragraphsCount);
            for (int p = 0; p < paragraphsCount; p++)
                descParts.Add(DescriptionParagraphs[(i + p) % DescriptionParagraphs.Length]);

            var description = string.Join("\n\n", descParts);

            var latJitter = (rng.NextDouble() - 0.5) * 0.02;
            var lngJitter = (rng.NextDouble() - 0.5) * 0.02;

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                CreatedAt = DateTime.UtcNow.AddDays(-rng.NextInt(220)),

                CityId = city.Id,
                InstitutionId = inst.Id,
                ProjectTypeId = type.Id,

                Location = new Point(city.Longitude + lngJitter, city.Latitude + latJitter) { SRID = 4326 }
            };

            projects.Add(project);

            var tagsCount = 1 + rng.NextInt(3);
            var chosen = new HashSet<int>();
            for (int t = 0; t < tagsCount; t++)
            {
                var tag = tags[rng.NextInt(tags.Count)];
                if (!chosen.Add(tag.Id)) { t--; continue; }

                projectTags.Add(new ProjectTag
                {
                    ProjectId = project.Id,
                    TagId = tag.Id
                });
            }
        }

        await db.Projects.AddRangeAsync(projects);
        await db.ProjectTags.AddRangeAsync(projectTags);
        await db.SaveChangesAsync();
    }
}
