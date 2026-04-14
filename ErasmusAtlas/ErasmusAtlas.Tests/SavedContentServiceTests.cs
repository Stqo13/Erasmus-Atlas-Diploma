using ErasmusAtlas.Core.Implementations;
using ErasmusAtlas.Infrastructure;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using ErasmusAtlas.Tests.TestHelpers;

using Microsoft.EntityFrameworkCore;

using Moq;

namespace ErasmusAtlas.Tests.Services;

public class SavedContentServiceTests
{
    private readonly Mock<IRepository<Post, Guid>> postRepositoryMock;
    private readonly Mock<IRepository<Project, Guid>> projectRepositoryMock;

    public SavedContentServiceTests()
    {
        postRepositoryMock = new Mock<IRepository<Post, Guid>>();
        projectRepositoryMock = new Mock<IRepository<Project, Guid>>();
    }

    [Fact]
    public async Task GetSavedPostsAsync_Should_Return_Saved_Posts_Ordered_By_SavedAt_Descending()
    {
        // Arrange
        using var db = CreateDbContext();

        var user = new ErasmusUser
        {
            Id = "user-1",
            UserName = "user1"
        };

        var city = new City
        {
            Id = 1,
            Name = "Madrid",
            CountryIso2 = "ES"
        };

        var topicHousing = new Topic
        {
            Id = 1,
            Name = "Housing"
        };

        var topicFood = new Topic
        {
            Id = 2,
            Name = "Food"
        };

        var olderPost = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Older saved post",
            Body = "Older body",
            UserId = "user-1",
            User = user,
            City = city,
            CityId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            PostTopics = new List<PostTopic>
            {
                new() { Topic = topicHousing, TopicId = 1 }
            }
        };

        var newerPost = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Newer saved post",
            Body = "Newer body",
            UserId = "user-1",
            User = user,
            City = city,
            CityId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            PostTopics = new List<PostTopic>
            {
                new() { Topic = topicFood, TopicId = 2 },
                new() { Topic = topicHousing, TopicId = 1 }
            }
        };

        db.Users.Add(user);
        db.Cities.Add(city);
        db.Topics.AddRange(topicHousing, topicFood);
        db.Posts.AddRange(olderPost, newerPost);

        db.SavedPosts.AddRange(
            new SavedPost
            {
                UserId = "user-1",
                User = user,
                PostId = olderPost.Id,
                Post = olderPost,
                SavedAt = DateTime.UtcNow.AddDays(-2)
            },
            new SavedPost
            {
                UserId = "user-1",
                User = user,
                PostId = newerPost.Id,
                Post = newerPost,
                SavedAt = DateTime.UtcNow.AddDays(-1)
            });

        await db.SaveChangesAsync();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = (await service.GetSavedPostsAsync("user-1")).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Newer saved post", result[0].Title);
        Assert.Equal("Older saved post", result[1].Title);
        Assert.Equal("Madrid", result[0].City);
        Assert.Equal(2, result[0].Topics.Count);
        Assert.Equal("Food", result[0].Topics[0]);
        Assert.Equal("Housing", result[0].Topics[1]);
    }

    [Fact]
    public async Task GetSavedProjectsAsync_Should_Return_Saved_Projects_Ordered_By_SavedAt_Descending()
    {
        // Arrange
        using var db = CreateDbContext();

        var user = new ErasmusUser
        {
            Id = "user-1",
            UserName = "user1"
        };

        var city = new City
        {
            Id = 1,
            Name = "Berlin",
            CountryIso2 = "DE"
        };

        var institution = new Institution
        {
            Id = 1,
            Name = "TU Berlin"
        };

        var projectType = new ProjectType
        {
            Id = 1,
            Name = "Research"
        };

        var tag = new Tag
        {
            Id = 1,
            Name = "AI"
        };

        var olderProject = new Project
        {
            Id = Guid.NewGuid(),
            Title = "Older project",
            Description = "Older description",
            City = city,
            CityId = 1,
            Institution = institution,
            ProjectType = projectType,
            ProjectTypeId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ProjectTags = new List<ProjectTag>
            {
                new() { Tag = tag, TagId = 1 }
            }
        };

        var newerProject = new Project
        {
            Id = Guid.NewGuid(),
            Title = "Newer project",
            Description = "Newer description",
            City = city,
            CityId = 1,
            Institution = institution,
            ProjectType = projectType,
            ProjectTypeId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            ProjectTags = new List<ProjectTag>
            {
                new() { Tag = tag, TagId = 1 }
            }
        };

        db.Users.Add(user);
        db.Cities.Add(city);
        db.Institutions.Add(institution);
        db.ProjectTypes.Add(projectType);
        db.Tags.Add(tag);
        db.Projects.AddRange(olderProject, newerProject);

        db.SavedProjects.AddRange(
            new SavedProject
            {
                UserId = "user-1",
                User = user,
                ProjectId = olderProject.Id,
                Project = olderProject,
                SavedAt = DateTime.UtcNow.AddDays(-2)
            },
            new SavedProject
            {
                UserId = "user-1",
                User = user,
                ProjectId = newerProject.Id,
                Project = newerProject,
                SavedAt = DateTime.UtcNow.AddDays(-1)
            });

        await db.SaveChangesAsync();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = (await service.GetSavedProjectsAsync("user-1")).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Newer project", result[0].Title);
        Assert.Equal("Older project", result[1].Title);
        Assert.Equal("Berlin", result[0].City);
        Assert.Equal("TU Berlin", result[0].Institution);
        Assert.Equal("Research", result[0].ProjectType);
        Assert.Single(result[0].Tags);
        Assert.Equal("AI", result[0].Tags[0]);
    }

    [Fact]
    public async Task IsPostSavedAsync_Should_Return_True_When_Post_Is_Saved()
    {
        // Arrange
        using var db = CreateDbContext();

        var postId = Guid.NewGuid();

        db.SavedPosts.Add(new SavedPost
        {
            UserId = "user-1",
            PostId = postId,
            SavedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.IsPostSavedAsync(postId, "user-1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsPostSavedAsync_Should_Return_False_When_Post_Is_Not_Saved()
    {
        // Arrange
        using var db = CreateDbContext();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.IsPostSavedAsync(Guid.NewGuid(), "user-1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsProjectSavedAsync_Should_Return_True_When_Project_Is_Saved()
    {
        // Arrange
        using var db = CreateDbContext();

        var projectId = Guid.NewGuid();

        db.SavedProjects.Add(new SavedProject
        {
            UserId = "user-1",
            ProjectId = projectId,
            SavedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.IsProjectSavedAsync(projectId, "user-1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsProjectSavedAsync_Should_Return_False_When_Project_Is_Not_Saved()
    {
        // Arrange
        using var db = CreateDbContext();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.IsProjectSavedAsync(Guid.NewGuid(), "user-1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SavePostAsync_Should_Return_False_When_Post_Does_Not_Exist()
    {
        // Arrange
        using var db = CreateDbContext();

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>().AsAsyncQueryable());

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.SavePostAsync(Guid.NewGuid(), "user-1");

        // Assert
        Assert.False(result);
        Assert.Empty(db.SavedPosts);
    }

    [Fact]
    public async Task SavePostAsync_Should_Add_SavedPost_When_Post_Exists_And_Is_Not_Already_Saved()
    {
        // Arrange
        using var db = CreateDbContext();

        var postId = Guid.NewGuid();

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>
            {
                new() { Id = postId, Title = "Post", Body = "Body", UserId = "owner" }
            }.AsAsyncQueryable());

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.SavePostAsync(postId, "user-1");

        // Assert
        Assert.True(result);
        Assert.Single(db.SavedPosts);
        Assert.Equal(postId, db.SavedPosts.First().PostId);
        Assert.Equal("user-1", db.SavedPosts.First().UserId);
    }

    [Fact]
    public async Task SavePostAsync_Should_Return_True_Without_Duplicating_When_Already_Saved()
    {
        // Arrange
        using var db = CreateDbContext();

        var postId = Guid.NewGuid();

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>
            {
                new() { Id = postId, Title = "Post", Body = "Body", UserId = "owner" }
            }.AsAsyncQueryable());

        db.SavedPosts.Add(new SavedPost
        {
            UserId = "user-1",
            PostId = postId,
            SavedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.SavePostAsync(postId, "user-1");

        // Assert
        Assert.True(result);
        Assert.Single(db.SavedPosts);
    }

    [Fact]
    public async Task SaveProjectAsync_Should_Return_False_When_Project_Does_Not_Exist()
    {
        // Arrange
        using var db = CreateDbContext();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Project>().AsAsyncQueryable());

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.SaveProjectAsync(Guid.NewGuid(), "user-1");

        // Assert
        Assert.False(result);
        Assert.Empty(db.SavedProjects);
    }

    [Fact]
    public async Task SaveProjectAsync_Should_Add_SavedProject_When_Project_Exists_And_Is_Not_Already_Saved()
    {
        // Arrange
        using var db = CreateDbContext();

        var projectId = Guid.NewGuid();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Project>
            {
                new()
                {
                    Id = projectId,
                    Title = "Project",
                    Description = "Description"
                }
            }.AsAsyncQueryable());

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.SaveProjectAsync(projectId, "user-1");

        // Assert
        Assert.True(result);
        Assert.Single(db.SavedProjects);
        Assert.Equal(projectId, db.SavedProjects.First().ProjectId);
        Assert.Equal("user-1", db.SavedProjects.First().UserId);
    }

    [Fact]
    public async Task SaveProjectAsync_Should_Return_True_Without_Duplicating_When_Already_Saved()
    {
        // Arrange
        using var db = CreateDbContext();

        var projectId = Guid.NewGuid();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Project>
            {
                new()
                {
                    Id = projectId,
                    Title = "Project",
                    Description = "Description"
                }
            }.AsAsyncQueryable());

        db.SavedProjects.Add(new SavedProject
        {
            UserId = "user-1",
            ProjectId = projectId,
            SavedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.SaveProjectAsync(projectId, "user-1");

        // Assert
        Assert.True(result);
        Assert.Single(db.SavedProjects);
    }

    [Fact]
    public async Task UnsavePostAsync_Should_Return_False_When_SavedPost_Does_Not_Exist()
    {
        // Arrange
        using var db = CreateDbContext();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.UnsavePostAsync(Guid.NewGuid(), "user-1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UnsavePostAsync_Should_Remove_SavedPost_When_It_Exists()
    {
        // Arrange
        using var db = CreateDbContext();

        var postId = Guid.NewGuid();

        db.SavedPosts.Add(new SavedPost
        {
            UserId = "user-1",
            PostId = postId,
            SavedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.UnsavePostAsync(postId, "user-1");

        // Assert
        Assert.True(result);
        Assert.Empty(db.SavedPosts);
    }

    [Fact]
    public async Task UnsaveProjectAsync_Should_Return_False_When_SavedProject_Does_Not_Exist()
    {
        // Arrange
        using var db = CreateDbContext();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.UnsaveProjectAsync(Guid.NewGuid(), "user-1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UnsaveProjectAsync_Should_Remove_SavedProject_When_It_Exists()
    {
        // Arrange
        using var db = CreateDbContext();

        var projectId = Guid.NewGuid();

        db.SavedProjects.Add(new SavedProject
        {
            UserId = "user-1",
            ProjectId = projectId,
            SavedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = new SavedContentService(
            postRepositoryMock.Object,
            projectRepositoryMock.Object,
            db);

        // Act
        var result = await service.UnsaveProjectAsync(projectId, "user-1");

        // Assert
        Assert.True(result);
        Assert.Empty(db.SavedProjects);
    }

    private static ErasmusAtlasDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ErasmusAtlasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ErasmusAtlasDbContext(options);
    }
}