using ErasmusAtlas.Core.Implementations;
using ErasmusAtlas.Infrastructure;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.ViewModels.AccountViewModels;
using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Tests.Services;

public class ProfileServiceTests
{
    [Fact]
    public async Task GetProfileAsync_Should_Return_Null_When_User_Does_Not_Exist()
    {
        var db = CreateDbContext();
        var service = new ProfileService(db);

        var result = await service.GetProfileAsync("missing-user");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetProfileAsync_Should_Return_Profile_With_Correct_Basic_Data()
    {
        var db = CreateDbContext();

        var user = new ErasmusUser
        {
            Id = "user-1",
            UserName = "ivan123",
            DisplayName = "Ivan",
            Bio = "Student in Madrid"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new ProfileService(db);

        var result = await service.GetProfileAsync("user-1", "user-1");

        Assert.NotNull(result);
        Assert.Equal("user-1", result!.UserId);
        Assert.Equal("ivan123", result.UserName);
        Assert.Equal("Ivan", result.DisplayName);
        Assert.Equal("Student in Madrid", result.Bio);
        Assert.True(result.IsOwner);
    }

    [Fact]
    public async Task GetProfileAsync_Should_Fallback_To_UserName_When_DisplayName_Is_Empty()
    {
        var db = CreateDbContext();

        var user = new ErasmusUser
        {
            Id = "user-1",
            UserName = "maria123",
            DisplayName = "",
            Bio = "Bio"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new ProfileService(db);

        var result = await service.GetProfileAsync("user-1", "other-user");

        Assert.NotNull(result);
        Assert.Equal("maria123", result!.DisplayName);
        Assert.False(result.IsOwner);
    }

    [Fact]
    public async Task GetProfileAsync_Should_Return_PostsCount_And_RecentPosts()
    {
        var db = CreateDbContext();

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

        var user = new ErasmusUser
        {
            Id = "user-1",
            UserName = "petar123",
            DisplayName = "Petar"
        };

        var olderPost = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Older post",
            Body = "Older body",
            UserId = "user-1",
            User = user,
            City = city,
            CityId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            PostTopics = new List<PostTopic>
            {
                new() { Topic = topicHousing, TopicId = 1 }
            }
        };

        var newerPost = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Newer post",
            Body = "Newer body",
            UserId = "user-1",
            User = user,
            City = city,
            CityId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
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

        await db.SaveChangesAsync();

        var service = new ProfileService(db);

        var result = await service.GetProfileAsync("user-1", "user-1");


        Assert.NotNull(result);

        var recentPosts = result.RecentPosts.ToList();

        Assert.Equal(2, result!.PostsCount);
        Assert.Equal(2, result.RecentPosts.Count());

        Assert.Equal("Newer post", recentPosts[0].Title);
        Assert.Equal("Older post", recentPosts[1].Title);

        Assert.Equal("Madrid", recentPosts[0].City);
        Assert.Equal(2, recentPosts[0].Topics.Count);
        Assert.Equal("Food", recentPosts[0].Topics[0]);
        Assert.Equal("Housing", recentPosts[0].Topics[1]);
    }

    [Fact]
    public async Task GetProfileAsync_Should_Return_Saved_Project_Preview_And_Counts()
    {
        var db = CreateDbContext();

        var city = new City
        {
            Id = 1,
            Name = "Berlin",
            CountryIso2 = "DE"
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

        var institution = new Institution
        {
            Id = 1,
            Name = "TU Berlin"
        };

        var user = new ErasmusUser
        {
            Id = "user-1",
            UserName = "alex123",
            DisplayName = "Alex",
            SavedProjects = new List<SavedProject>(),
            SavedPosts = new List<SavedPost>()
        };

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = "AI Lab",
            Description = "A short description for the project.",
            City = city,
            CityId = 1,
            Institution = institution,
            ProjectType = projectType,
            ProjectTypeId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ProjectTags = new List<ProjectTag>
            {
                new()
                {
                    Tag = tag,
                    TagId = 1
                }
            }
        };

        var savedProject = new SavedProject
        {
            UserId = "user-1",
            User = user,
            ProjectId = project.Id,
            Project = project,
            SavedAt = DateTime.UtcNow
        };

        var savedPost = new SavedPost
        {
            UserId = "user-1",
            PostId = Guid.NewGuid(),
            SavedAt = DateTime.UtcNow
        };

        user.SavedProjects.Add(savedProject);
        user.SavedPosts.Add(savedPost);

        db.Users.Add(user);
        db.Cities.Add(city);
        db.ProjectTypes.Add(projectType);
        db.Tags.Add(tag);
        db.Institutions.Add(institution);
        db.Projects.Add(project);
        db.SavedProjects.Add(savedProject);
        db.SavedPosts.Add(savedPost);

        await db.SaveChangesAsync();

        var service = new ProfileService(db);

        var result = await service.GetProfileAsync("user-1", "user-1");

        Assert.NotNull(result);

        var savedProjectsPreview = result.SavedProjectsPreview.ToList();

        Assert.Equal(1, result!.SavedProjectsCount);
        Assert.Equal(1, result.SavedPostsCount);
        Assert.Single(result.SavedProjectsPreview);

        var preview = savedProjectsPreview[0];
        Assert.Equal("AI Lab", preview.Title);
        Assert.Equal("Berlin", preview.City);
        Assert.Equal("TU Berlin", preview.Institution);
        Assert.Equal("Research", preview.ProjectType);
        Assert.Single(preview.Tags);
        Assert.Equal("AI", preview.Tags[0]);
    }

    [Fact]
    public async Task GetEditModelAsync_Should_Return_Edit_Model_When_User_Exists()
    {
        var db = CreateDbContext();

        db.Users.Add(new ErasmusUser
        {
            Id = "user-1",
            UserName = "testuser",
            DisplayName = "Test User",
            Bio = "Hello there"
        });

        await db.SaveChangesAsync();

        var service = new ProfileService(db);

        var result = await service.GetEditModelAsync("user-1");

        Assert.NotNull(result);
        Assert.Equal("Test User", result!.DisplayName);
        Assert.Equal("Hello there", result.Bio);
    }

    [Fact]
    public async Task GetEditModelAsync_Should_Return_Null_When_User_Does_Not_Exist()
    {
        var db = CreateDbContext();
        var service = new ProfileService(db);

        var result = await service.GetEditModelAsync("missing-user");

        Assert.Null(result);
    }

    [Fact]
    public async Task EditAsync_Should_Return_False_When_User_Does_Not_Exist()
    {
        var db = CreateDbContext();
        var service = new ProfileService(db);

        var result = await service.EditAsync("missing-user", new EditProfileViewModel
        {
            DisplayName = "Changed",
            Bio = "Changed bio"
        });

        Assert.False(result);
    }

    [Fact]
    public async Task EditAsync_Should_Update_User_When_User_Exists()
    {
        var db = CreateDbContext();

        db.Users.Add(new ErasmusUser
        {
            Id = "user-1",
            UserName = "user1",
            DisplayName = "Old Name",
            Bio = "Old Bio"
        });

        await db.SaveChangesAsync();

        var service = new ProfileService(db);

        var result = await service.EditAsync("user-1", new EditProfileViewModel
        {
            DisplayName = "New Name",
            Bio = "New Bio"
        });

        var updatedUser = await db.Users.FirstAsync(u => u.Id == "user-1");

        Assert.True(result);
        Assert.Equal("New Name", updatedUser.DisplayName);
        Assert.Equal("New Bio", updatedUser.Bio);
    }

    private static ErasmusAtlasDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ErasmusAtlasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ErasmusAtlasDbContext(options);
    }
}