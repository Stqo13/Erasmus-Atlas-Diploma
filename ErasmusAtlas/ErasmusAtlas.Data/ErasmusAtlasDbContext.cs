using System.Reflection;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using ErasmusAtlas.Infrastructure.Models;

namespace ErasmusAtlas.Infrastructure;

public class ErasmusAtlasDbContext : IdentityDbContext
{

    public ErasmusAtlasDbContext(DbContextOptions<ErasmusAtlasDbContext> options)
        : base(options) { }

    public virtual DbSet<City> Cities { get; set; }
    public virtual DbSet<Post> Posts { get; set; }
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<ProjectType> ProjectTypes { get; set; }
    public virtual DbSet<Institution> Institutions { get; set; }
    public virtual DbSet<ProjectApplication> ProjectApplications { get; set; }
    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<Topic> Topics { get; set; }
    public virtual DbSet<ProjectTag> ProjectTags { get; set; }
    public virtual DbSet<PostTopic> PostTopics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }
}
