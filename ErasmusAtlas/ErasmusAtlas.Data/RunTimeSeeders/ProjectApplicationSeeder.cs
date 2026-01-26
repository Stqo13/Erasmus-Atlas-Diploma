using ErasmusAtlas.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Infrastructure.RunTimeSeeders;

public static class ProjectApplicationSeeder
{
    private static readonly string[] Statuses = { "Pending", "Approved", "Rejected" };

    private static readonly string[] MotivationSnippets =
    [
        "I’m excited to learn in an international environment and contribute to the team.",
        "I want to improve my practical skills and collaborate with students from different backgrounds.",
        "This project matches my interests and I’m ready to commit consistently throughout the program.",
        "I’m motivated to represent my university well and gain experience I can use in my future career.",
        "I enjoy structured work and I’m confident I can meet deadlines and participate actively."
    ];

    public static async Task SeedAsync(ErasmusAtlasDbContext db, IReadOnlyList<string> userIds, int maxApplicationsPerProject = 3)
    {
        if (await db.ProjectApplications.AnyAsync())
            return;

        if (userIds == null || userIds.Count == 0)
            throw new ArgumentException("userIds must contain at least one user.");

        var projects = await db.Projects.OrderBy(p => p.CreatedAt).ToListAsync();
        if (projects.Count == 0)
            throw new Exception("No projects found. Seed projects first.");

        var rng = new Random(7070);

        var applications = new List<ProjectApplication>();

        foreach (var project in projects)
        {
            var applicationsCount = rng.Next(0, maxApplicationsPerProject + 1);

            var usedUsers = new HashSet<string>();
            for (int i = 0; i < applicationsCount; i++)
            {
                var userId = userIds[rng.Next(userIds.Count)];
                if (!usedUsers.Add(userId)) { i--; continue; }

                var status = Statuses[rng.Next(Statuses.Length)];
                var motivation = rng.NextDouble() < 0.85
                    ? MotivationSnippets[rng.Next(MotivationSnippets.Length)]
                    : null;

                applications.Add(new ProjectApplication
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    UserId = userId,
                    Status = status,
                    Motivation = motivation,
                    CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(120))
                });
            }
        }

        await db.ProjectApplications.AddRangeAsync(applications);
        await db.SaveChangesAsync();
    }
}
