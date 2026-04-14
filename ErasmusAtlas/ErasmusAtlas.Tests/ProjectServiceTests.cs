using ErasmusAtlas.Core.Implementations;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using ErasmusAtlas.Tests.TestHelpers;
using ErasmusAtlas.ViewModels.ProjectViewModels;

using Moq;

namespace ErasmusAtlas.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IRepository<Project, Guid>> projectRepositoryMock;
    private readonly Mock<IRepository<City, int>> cityRepositoryMock;
    private readonly Mock<IRepository<ProjectType, int>> projectTypeRepositoryMock;
    private readonly Mock<IRepository<Tag, int>> tagRepositoryMock;
    private readonly Mock<IRepository<SavedProject, object>> savedProjectRepositoryMock;

    private readonly ProjectService service;

    public ProjectServiceTests()
    {
        projectRepositoryMock = new Mock<IRepository<Project, Guid>>();
        cityRepositoryMock = new Mock<IRepository<City, int>>();
        projectTypeRepositoryMock = new Mock<IRepository<ProjectType, int>>();
        tagRepositoryMock = new Mock<IRepository<Tag, int>>();
        savedProjectRepositoryMock = new Mock<IRepository<SavedProject, object>>();

        cityRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<City>
            {
                new() { Id = 1, Name = "Sofia" },
                new() { Id = 2, Name = "Madrid" }
            }.AsAsyncQueryable());

        projectTypeRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<ProjectType>
            {
                new() { Id = 1, Name = "Research" },
                new() { Id = 2, Name = "Internship" }
            }.AsAsyncQueryable());

        tagRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Tag>
            {
                new() { Id = 1, Name = "AI" },
                new() { Id = 2, Name = "Web" },
                new() { Id = 3, Name = "Erasmus+" }
            }.AsAsyncQueryable());

        savedProjectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<SavedProject>().AsAsyncQueryable());

        service = new ProjectService(
            projectRepositoryMock.Object,
            cityRepositoryMock.Object,
            projectTypeRepositoryMock.Object,
            tagRepositoryMock.Object,
            savedProjectRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Return_All_When_No_Filters()
    {
        var projects = BuildProjects();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(projects.AsAsyncQueryable());

        var filter = new ProjectFilterViewModel
        {
            Page = 1,
            PageSize = 10
        };

        var result = await service.GetAllFilteredAsync(filter);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Projects.Count());
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Filter_By_SearchTerm()
    {
        var projects = BuildProjects();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(projects.AsAsyncQueryable());

        var filter = new ProjectFilterViewModel
        {
            SearchTerm = "Climate",
            Page = 1,
            PageSize = 10
        };

        var result = await service.GetAllFilteredAsync(filter);

        var project = Assert.Single(result.Projects);
        Assert.Equal("Climate Lab", project.Title);
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Filter_By_CityId()
    {
        var projects = BuildProjects();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(projects.AsAsyncQueryable());

        var filter = new ProjectFilterViewModel
        {
            CityId = 2,
            Page = 1,
            PageSize = 10
        };

        var result = await service.GetAllFilteredAsync(filter);

        Assert.Equal(2, result.Projects.Count());
        Assert.All(result.Projects, p => Assert.Equal("Madrid", p.City));
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Filter_By_ProjectTypeId()
    {
        var projects = BuildProjects();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(projects.AsAsyncQueryable());

        var filter = new ProjectFilterViewModel
        {
            ProjectTypeId = 1,
            Page = 1,
            PageSize = 10
        };

        var result = await service.GetAllFilteredAsync(filter);

        Assert.Equal(2, result.Projects.Count());
        Assert.All(result.Projects, p => Assert.Equal("Research", p.ProjectType));
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Filter_By_TagIds()
    {
        var projects = BuildProjects();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(projects.AsAsyncQueryable());

        var filter = new ProjectFilterViewModel
        {
            TagIds = new List<int> { 1 },
            Page = 1,
            PageSize = 10
        };

        var result = await service.GetAllFilteredAsync(filter);

        Assert.Equal(2, result.Projects.Count());
        Assert.Contains(result.Projects, p => p.Title == "AI for Campus");
        Assert.Contains(result.Projects, p => p.Title == "Climate Lab");
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Apply_Pagination()
    {
        var projects = BuildProjects();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(projects.AsAsyncQueryable());

        var filter = new ProjectFilterViewModel
        {
            Page = 2,
            PageSize = 1
        };

        var result = await service.GetAllFilteredAsync(filter);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(1, result.PageSize);
        Assert.Single(result.Projects);
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Map_DescriptionPreview_Truncated()
    {
        var longDescription = new string('A', 250);

        var projects = new List<Project>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Long Description Project",
                Description = longDescription,
                CityId = 1,
                City = new City { Id = 1, Name = "Sofia" },
                ProjectTypeId = 1,
                ProjectType = new ProjectType { Id = 1, Name = "Research" },
                Institution = new Institution { Name = "SoftUni" },
                CreatedAt = DateTime.UtcNow,
                ProjectTags = new List<ProjectTag>()
            }
        };

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(projects.AsAsyncQueryable());

        var filter = new ProjectFilterViewModel
        {
            Page = 1,
            PageSize = 10
        };

        var result = await service.GetAllFilteredAsync(filter);

        var project = Assert.Single(result.Projects);
        Assert.True(project.DescriptionPreview.Length <= 223);
        Assert.EndsWith("...", project.DescriptionPreview);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Details_With_IsSaved_True()
    {
        var projectId = Guid.NewGuid();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Project>
            {
                new()
                {
                    Id = projectId,
                    Title = "AI for Campus",
                    Description = "Full description",
                    City = new City { Id = 2, Name = "Madrid" },
                    Institution = new Institution { Name = "UCM" },
                    ProjectType = new ProjectType { Id = 1, Name = "Research" },
                    CreatedAt = DateTime.UtcNow,
                    Location = null,
                    ProjectTags = new List<ProjectTag>
                    {
                        new() { Tag = new Tag { Id = 1, Name = "AI" } }
                    }
                }
            }.AsAsyncQueryable());

        savedProjectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<SavedProject>
            {
                new() { UserId = "user-1", ProjectId = projectId }
            }.AsAsyncQueryable());

        var result = await service.GetByIdAsync(projectId, "user-1");

        Assert.NotNull(result);
        Assert.Equal(projectId, result!.Id);
        Assert.Equal("AI for Campus", result.Title);
        Assert.Equal("Full description", result.Description);
        Assert.Equal("Madrid", result.City);
        Assert.Equal("UCM", result.Institution);
        Assert.Equal("Research", result.ProjectType);
        Assert.True(result.IsSaved);
        Assert.Contains("AI", result.Tags);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Map_Location_Correctly()
    {
        var projectId = Guid.NewGuid();

        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Project>
            {
                new()
                {
                    Id = projectId,
                    Title = "Mapped location",
                    Description = "Description",
                    City = new City { Id = 1, Name = "Sofia" },
                    Institution = new Institution { Name = "SoftUni" },
                    ProjectType = new ProjectType { Id = 1, Name = "Research" },
                    CreatedAt = DateTime.UtcNow,
                    Location = new NetTopologySuite.Geometries.Point(23.3219, 42.6977) { SRID = 4326 },
                    ProjectTags = new List<ProjectTag>()
                }
            }.AsAsyncQueryable());

        savedProjectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<SavedProject>().AsAsyncQueryable());

        var result = await service.GetByIdAsync(projectId, "user-1");

        Assert.NotNull(result);
        Assert.NotNull(result!.Latitude);
        Assert.NotNull(result.Longitude);
        Assert.Equal(42.6977, result.Latitude.Value, 4);
        Assert.Equal(23.3219, result.Longitude.Value, 4);
    }

    [Fact]
    public async Task GetCitiesAsync_Should_Return_Cities_Ordered_By_Name()
    {
        var result = (await service.GetCitiesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Madrid", result[0].Name);
        Assert.Equal("Sofia", result[1].Name);
    }

    [Fact]
    public async Task GetLatestAsync_Should_Return_Latest_Projects_First()
    {
        projectRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Project>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Older",
                    Description = "Old description",
                    City = new City { Id = 1, Name = "Sofia" },
                    Institution = new Institution { Name = "SoftUni" },
                    ProjectType = new ProjectType { Id = 1, Name = "Research" },
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    ProjectTags = new List<ProjectTag>()
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Newest",
                    Description = "New description",
                    City = new City { Id = 2, Name = "Madrid" },
                    Institution = new Institution { Name = "UCM" },
                    ProjectType = new ProjectType { Id = 2, Name = "Internship" },
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    ProjectTags = new List<ProjectTag>()
                }
            }.AsAsyncQueryable());

        var result = (await service.GetLatestAsync(1)).ToList();

        Assert.Single(result);
        Assert.Equal("Newest", result[0].Title);
    }

    [Fact]
    public async Task GetProjectTypesAsync_Should_Return_ProjectTypes_Ordered_By_Name()
    {
        var result = (await service.GetProjectTypesAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Internship", result[0].Name);
        Assert.Equal("Research", result[1].Name);
    }

    [Fact]
    public async Task GetTagsAsync_Should_Return_Tags_Ordered_By_Name()
    {
        var result = (await service.GetTagsAsync()).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal("AI", result[0].Name);
        Assert.Equal("Erasmus+", result[1].Name);
        Assert.Equal("Web", result[2].Name);
    }

    private static List<Project> BuildProjects()
    {
        return new List<Project>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "AI for Campus",
                Description = "AI project for students",
                CityId = 1,
                City = new City { Id = 1, Name = "Sofia" },
                ProjectTypeId = 1,
                ProjectType = new ProjectType { Id = 1, Name = "Research" },
                Institution = new Institution { Name = "SoftUni" },
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ProjectTags = new List<ProjectTag>
                {
                    new() { TagId = 1, Tag = new Tag { Id = 1, Name = "AI" } },
                    new() { TagId = 3, Tag = new Tag { Id = 3, Name = "Erasmus+" } }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Climate Lab",
                Description = "Climate research and sustainability",
                CityId = 2,
                City = new City { Id = 2, Name = "Madrid" },
                ProjectTypeId = 1,
                ProjectType = new ProjectType { Id = 1, Name = "Research" },
                Institution = new Institution { Name = "UCM" },
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ProjectTags = new List<ProjectTag>
                {
                    new() { TagId = 1, Tag = new Tag { Id = 1, Name = "AI" } }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Frontend Internship",
                Description = "Build web interfaces for university tools",
                CityId = 2,
                City = new City { Id = 2, Name = "Madrid" },
                ProjectTypeId = 2,
                ProjectType = new ProjectType { Id = 2, Name = "Internship" },
                Institution = new Institution { Name = "Tech Hub" },
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ProjectTags = new List<ProjectTag>
                {
                    new() { TagId = 2, Tag = new Tag { Id = 2, Name = "Web" } }
                }
            }
        };
    }
}