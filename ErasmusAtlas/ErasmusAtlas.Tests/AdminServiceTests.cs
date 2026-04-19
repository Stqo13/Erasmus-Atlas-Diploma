using ErasmusAtlas.Core.Implementations;
using ErasmusAtlas.Infrastructure;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.Tests.TestHelpers;
using ErasmusAtlas.ViewModels.AdminViewModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Moq;

namespace ErasmusAtlas.Tests.Services;

public class AdminServiceTests
{
    [Fact]
    public async Task GetDashboardAsync_Should_Return_Counts_And_Latest_Content()
    {
        using var db = CreateDbContext();

        var adminUser = new ErasmusUser
        {
            Id = "admin-1",
            UserName = "admin",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            DisplayName = "Admin User"
        };

        var normalUser = new ErasmusUser
        {
            Id = "user-1",
            UserName = "user1",
            Email = "user1@test.com",
            FirstName = "Ivan",
            LastName = "Petrov",
            DisplayName = "Ivan"
        };

        var city = new City
        {
            Id = 1,
            Name = "Madrid",
            CountryIso2 = "ES"
        };

        var topic = new Topic
        {
            Id = 1,
            Name = "Housing"
        };

        var institution = new Institution
        {
            Id = 1,
            Name = "UCM"
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

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Latest Post",
            Body = "Post body",
            UserId = normalUser.Id,
            User = normalUser,
            CityId = 1,
            City = city,
            CreatedAt = DateTime.UtcNow,
            PostTopics = new List<PostTopic>
            {
                new() { Topic = topic, TopicId = 1 }
            }
        };

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = "Latest Project",
            Description = "Project description",
            CityId = 1,
            City = city,
            Institution = institution,
            ProjectType = projectType,
            ProjectTypeId = 1,
            CreatedAt = DateTime.UtcNow,
            ProjectTags = new List<ProjectTag>
            {
                new() { Tag = tag, TagId = 1 }
            }
        };

        db.Users.AddRange(adminUser, normalUser);
        db.Cities.Add(city);
        db.Topics.Add(topic);
        db.Institutions.Add(institution);
        db.ProjectTypes.Add(projectType);
        db.Tags.Add(tag);
        db.Posts.Add(post);
        db.Projects.Add(project);
        db.SavedPosts.Add(new SavedPost
        {
            UserId = normalUser.Id,
            PostId = post.Id,
            SavedAt = DateTime.UtcNow
        });
        db.SavedProjects.Add(new SavedProject
        {
            UserId = normalUser.Id,
            ProjectId = project.Id,
            SavedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var userManager = CreateUserManager(adminUser, normalUser);
        var roleManager = CreateRoleManager("Admin", "User");

        userManager.Setup(x => x.GetRolesAsync(It.Is<ErasmusUser>(u => u.Id == "admin-1")))
            .ReturnsAsync(new List<string> { "Admin" });

        userManager.Setup(x => x.GetRolesAsync(It.Is<ErasmusUser>(u => u.Id == "user-1")))
            .ReturnsAsync(new List<string> { "User" });

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.GetDashboardAsync();

        Assert.Equal(2, result.UsersCount);
        Assert.Equal(1, result.PostsCount);
        Assert.Equal(1, result.ProjectsCount);
        Assert.Equal(1, result.SavedPostsCount);
        Assert.Equal(1, result.SavedProjectsCount);

        Assert.Single(result.LatestPosts);
        Assert.Equal("Latest Post", result.LatestPosts.First().Title);

        Assert.Single(result.LatestProjects);
        Assert.Equal("Latest Project", result.LatestProjects.First().Title);

        Assert.Equal(2, result.LatestUsers.Count());
    }

    [Fact]
    public async Task GetUsersAsync_Should_Return_All_Users_With_Roles()
    {
        using var db = CreateDbContext();

        var adminUser = new ErasmusUser
        {
            Id = "admin-1",
            UserName = "admin",
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "User",
            DisplayName = "Admin User"
        };

        var normalUser = new ErasmusUser
        {
            Id = "user-1",
            UserName = "user1",
            Email = "user1@test.com",
            FirstName = "Ivan",
            LastName = "Petrov",
            DisplayName = "Ivan"
        };

        db.Users.AddRange(adminUser, normalUser);
        await db.SaveChangesAsync();

        var userManager = CreateUserManager(adminUser, normalUser);
        var roleManager = CreateRoleManager("Admin", "User");

        userManager.Setup(x => x.GetRolesAsync(It.Is<ErasmusUser>(u => u.Id == "admin-1")))
            .ReturnsAsync(new List<string> { "Admin" });

        userManager.Setup(x => x.GetRolesAsync(It.Is<ErasmusUser>(u => u.Id == "user-1")))
            .ReturnsAsync(new List<string> { "User" });

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.GetUsersAsync();

        Assert.Equal(2, result.Users.Count());
        Assert.Contains(result.Users, u => u.UserName == "admin" && u.Roles.Contains("Admin"));
        Assert.Contains(result.Users, u => u.UserName == "user1" && u.Roles.Contains("User"));
    }

    [Fact]
    public async Task GetEditUserAsync_Should_Return_Null_When_User_Does_Not_Exist()
    {
        using var db = CreateDbContext();

        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager("Admin", "User");

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.GetEditUserAsync("missing");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetEditUserAsync_Should_Return_User_Model_With_Roles()
    {
        using var db = CreateDbContext();

        var user = new ErasmusUser
        {
            Id = "user-1",
            UserName = "user1",
            Email = "user1@test.com",
            FirstName = "Ivan",
            LastName = "Petrov",
            DisplayName = "Ivan",
            Bio = "Bio here"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var userManager = CreateUserManager(user);
        var roleManager = CreateRoleManager("Admin", "User", "Moderator");

        userManager.Setup(x => x.GetRolesAsync(It.Is<ErasmusUser>(u => u.Id == "user-1")))
            .ReturnsAsync(new List<string> { "User", "Moderator" });

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.GetEditUserAsync("user-1");

        Assert.NotNull(result);
        Assert.Equal("user1@test.com", result!.Email);
        Assert.Equal("user1", result.UserName);
        Assert.Equal("Ivan", result.FirstName);
        Assert.Equal("Petrov", result.LastName);
        Assert.Equal("Ivan", result.DisplayName);
        Assert.Equal("Bio here", result.Bio);
        Assert.Contains("User", result.CurrentRoles);
        Assert.Contains("Moderator", result.CurrentRoles);
        Assert.Equal(3, result.AllRoles.Count);
    }

    [Fact]
    public async Task EditUserAsync_Should_Return_False_When_User_Not_Found()
    {
        using var db = CreateDbContext();

        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager("Admin", "User");

        userManager.Setup(x => x.FindByIdAsync("missing"))
            .ReturnsAsync((ErasmusUser?)null);

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.EditUserAsync(new AdminEditUserViewModel
        {
            Id = "missing",
            Email = "x@test.com",
            UserName = "x",
            FirstName = "X",
            LastName = "Y",
            SelectedRoles = new List<string> { "User" }
        });

        Assert.False(result);
    }

    [Fact]
    public async Task EditUserAsync_Should_Update_User_And_Roles()
    {
        using var db = CreateDbContext();

        var user = new ErasmusUser
        {
            Id = "user-1",
            UserName = "olduser",
            Email = "old@test.com",
            FirstName = "Old",
            LastName = "Name",
            DisplayName = "Old Display",
            Bio = "Old Bio"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var userManager = CreateUserManager(user);
        var roleManager = CreateRoleManager("Admin", "User", "Moderator");

        userManager.Setup(x => x.FindByIdAsync("user-1"))
            .ReturnsAsync(user);

        userManager.Setup(x => x.UpdateAsync(It.IsAny<ErasmusUser>()))
            .ReturnsAsync(IdentityResult.Success);

        userManager.Setup(x => x.GetRolesAsync(It.Is<ErasmusUser>(u => u.Id == "user-1")))
            .ReturnsAsync(new List<string> { "User" });

        userManager.Setup(x => x.RemoveFromRolesAsync(It.IsAny<ErasmusUser>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        userManager.Setup(x => x.AddToRolesAsync(It.IsAny<ErasmusUser>(), It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var model = new AdminEditUserViewModel
        {
            Id = "user-1",
            Email = "new@test.com",
            UserName = "newuser",
            FirstName = "New",
            LastName = "Person",
            DisplayName = "New Display",
            Bio = "New Bio",
            SelectedRoles = new List<string> { "Admin", "Moderator" }
        };

        var result = await service.EditUserAsync(model);

        Assert.True(result);
        Assert.Equal("new@test.com", user.Email);
        Assert.Equal("newuser", user.UserName);
        Assert.Equal("New", user.FirstName);
        Assert.Equal("Person", user.LastName);
        Assert.Equal("New Display", user.DisplayName);
        Assert.Equal("New Bio", user.Bio);

        userManager.Verify(x => x.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("User"))), Times.Once);
        userManager.Verify(x => x.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("Admin") && r.Contains("Moderator"))), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_Should_Return_False_When_User_Not_Found()
    {
        using var db = CreateDbContext();

        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager("Admin", "User");

        userManager.Setup(x => x.FindByIdAsync("missing"))
            .ReturnsAsync((ErasmusUser?)null);

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.DeleteUserAsync("missing");

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteUserAsync_Should_Return_True_When_Delete_Succeeds()
    {
        using var db = CreateDbContext();

        var user = new ErasmusUser
        {
            Id = "user-1",
            UserName = "user1",
            Email = "user1@test.com"
        };

        var userManager = CreateUserManager(user);
        var roleManager = CreateRoleManager("Admin", "User");

        userManager.Setup(x => x.FindByIdAsync("user-1"))
            .ReturnsAsync(user);

        userManager.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.DeleteUserAsync("user-1");

        Assert.True(result);
    }

    [Fact]
    public async Task GetPostsAsync_Should_Return_Posts()
    {
        using var db = CreateDbContext();

        var city = new City
        {
            Id = 1,
            Name = "Madrid",
            CountryIso2 = "ES"
        };

        var topic = new Topic
        {
            Id = 1,
            Name = "Housing"
        };

        var user = new ErasmusUser
        {
            Id = "user-1",
            UserName = "user1"
        };

        db.Users.Add(user);
        db.Cities.Add(city);
        db.Topics.Add(topic);

        db.Posts.Add(new Post
        {
            Id = Guid.NewGuid(),
            Title = "Admin post",
            Body = "Body",
            UserId = "user-1",
            User = user,
            City = city,
            CityId = 1,
            CreatedAt = DateTime.UtcNow,
            PostTopics = new List<PostTopic>
            {
                new() { Topic = topic, TopicId = 1 }
            }
        });

        await db.SaveChangesAsync();

        var userManager = CreateUserManager(user);
        var roleManager = CreateRoleManager("Admin");

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.GetPostsAsync();

        Assert.Single(result.Posts);
        Assert.Equal("Admin post", result.Posts.First().Title);
    }

    [Fact]
    public async Task GetProjectsAsync_Should_Return_Projects()
    {
        using var db = CreateDbContext();

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

        db.Cities.Add(city);
        db.Institutions.Add(institution);
        db.ProjectTypes.Add(projectType);
        db.Tags.Add(tag);

        db.Projects.Add(new Project
        {
            Id = Guid.NewGuid(),
            Title = "Admin project",
            Description = "Description",
            City = city,
            CityId = 1,
            Institution = institution,
            ProjectType = projectType,
            ProjectTypeId = 1,
            CreatedAt = DateTime.UtcNow,
            ProjectTags = new List<ProjectTag>
            {
                new() { Tag = tag, TagId = 1 }
            }
        });

        await db.SaveChangesAsync();

        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager("Admin");

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.GetProjectsAsync();

        Assert.Single(result.Projects);
        Assert.Equal("Admin project", result.Projects.First().Title);
    }

    [Fact]
    public async Task DeletePostAsync_Should_Return_False_When_Post_Not_Found()
    {
        using var db = CreateDbContext();

        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager("Admin");

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.DeletePostAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task DeletePostAsync_Should_Delete_Post_When_Found()
    {
        using var db = CreateDbContext();

        var post = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Delete me",
            Body = "Body",
            UserId = "user-1"
        };

        db.Posts.Add(post);
        await db.SaveChangesAsync();

        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager("Admin");

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.DeletePostAsync(post.Id);

        Assert.True(result);
        Assert.Empty(db.Posts);
    }

    [Fact]
    public async Task DeleteProjectAsync_Should_Return_False_When_Project_Not_Found()
    {
        using var db = CreateDbContext();

        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager("Admin");

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.DeleteProjectAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteProjectAsync_Should_Delete_Project_When_Found()
    {
        using var db = CreateDbContext();

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = "Delete project",
            Description = "Description"
        };

        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var userManager = CreateUserManager();
        var roleManager = CreateRoleManager("Admin");

        var service = new AdminService(db, userManager.Object, roleManager.Object);

        var result = await service.DeleteProjectAsync(project.Id);

        Assert.True(result);
        Assert.Empty(db.Projects);
    }

    private static ErasmusAtlasDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ErasmusAtlasDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ErasmusAtlasDbContext(options);
    }

    private static Mock<UserManager<ErasmusUser>> CreateUserManager(params ErasmusUser[] users)
    {
        var store = new Mock<IUserStore<ErasmusUser>>();
        var mgr = new Mock<UserManager<ErasmusUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

        if (users.Length > 0)
        {
            foreach (var user in users)
            {
                mgr.Setup(x => x.FindByIdAsync(user.Id))
                    .ReturnsAsync(user);
            }
        }

        return mgr;
    }

    private static Mock<RoleManager<IdentityRole>> CreateRoleManager(params string[] roles)
    {
        var store = new Mock<IRoleStore<IdentityRole>>();
        var mgr = new Mock<RoleManager<IdentityRole>>(
            store.Object,
            null!, null!, null!, null!);

        var queryableRoles = roles
            .Select(r => new IdentityRole(r))
            .AsAsyncQueryable();

        mgr.Setup(x => x.Roles).Returns(queryableRoles);

        return mgr;
    }
}