using ErasmusAtlas.Core.Implementations;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using ErasmusAtlas.Tests.TestHelpers;

using Moq;

namespace ErasmusAtlas.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IRepository<Post, Guid>> postRepositoryMock;
    private readonly Mock<IRepository<City, int>> cityRepositoryMock;

    private readonly DashboardService service;

    public DashboardServiceTests()
    {
        postRepositoryMock = new Mock<IRepository<Post, Guid>>();
        cityRepositoryMock = new Mock<IRepository<City, int>>();

        service = new DashboardService(
            postRepositoryMock.Object,
            cityRepositoryMock.Object);
    }

    [Fact]
    public async Task GetPageAsync_Should_Return_Country_Options_Based_On_Current_Logic()
    {
        // Arrange
        var cities = new List<City>
        {
            new() { Id = 1, Name = "Madrid", CountryIso2 = "ES" },
            new() { Id = 2, Name = "Barcelona", CountryIso2 = "ES" },
            new() { Id = 3, Name = "Rome", CountryIso2 = "IT" }
        };

        cityRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(cities.AsAsyncQueryable());

        // Act
        var result = await service.GetPageAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Countries.Count);

        Assert.Contains(result.Countries, c => c.Iso2 == "ES" && c.Name == "Madrid");
        Assert.Contains(result.Countries, c => c.Iso2 == "ES" && c.Name == "Barcelona");
        Assert.Contains(result.Countries, c => c.Iso2 == "IT" && c.Name == "Rome");
    }

    [Fact]
    public async Task GetOverviewAsync_Should_Return_Zeroed_Data_When_No_Posts()
    {
        // Arrange
        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>().AsAsyncQueryable());

        // Act
        var result = await service.GetOverviewAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalPosts);
        Assert.Equal(0, result.ActiveCountries);
        Assert.Equal(0, result.ActiveCities);
        Assert.Equal(0, result.PostsThisMonth);
        Assert.Equal(0, result.GrowthRate);
        Assert.Equal("N/A", result.TopTopic);
        Assert.Equal(0, result.TopicDiversityScore);
        Assert.Empty(result.Topics);
        Assert.Equal(12, result.Series.Count);
        Assert.NotEmpty(result.Insights);
    }

    [Fact]
    public async Task GetOverviewAsync_Should_Return_TotalPosts_ActiveCountries_And_ActiveCities()
    {
        // Arrange
        var posts = BuildPosts();

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetOverviewAsync();

        // Assert
        Assert.Equal(4, result.TotalPosts);
        Assert.Equal(2, result.ActiveCountries);
        Assert.Equal(3, result.ActiveCities);
    }

    [Fact]
    public async Task GetOverviewAsync_Should_Filter_By_CountryIso2()
    {
        // Arrange
        var posts = BuildPosts();

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetOverviewAsync("ES");

        // Assert
        Assert.Equal(2, result.TotalPosts);
        Assert.Equal(1, result.ActiveCountries);
        Assert.Equal(2, result.ActiveCities);
        Assert.All(result.Topics, t => Assert.True(t.Count >= 0));
    }

    [Fact]
    public async Task GetOverviewAsync_Should_Return_TopTopic()
    {
        // Arrange
        var posts = BuildPosts();

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetOverviewAsync();

        // Assert
        Assert.Equal("Housing", result.TopTopic);
    }

    [Fact]
    public async Task GetOverviewAsync_Should_Return_TopicPercentages_In_Valid_Range()
    {
        // Arrange
        var posts = BuildPosts();

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetOverviewAsync();

        // Assert
        Assert.NotEmpty(result.Topics);
        Assert.All(result.Topics, t => Assert.InRange(t.Percentage, 0, 100));
        Assert.Contains(result.Topics, t => t.Topic == "Housing");
        Assert.Contains(result.Topics, t => t.Topic == "Costs");
        Assert.Contains(result.Topics, t => t.Topic == "Food");
    }

    [Fact]
    public async Task GetOverviewAsync_Should_Return_12MonthSeries()
    {
        // Arrange
        var posts = BuildPosts();

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetOverviewAsync();

        // Assert
        Assert.Equal(12, result.Series.Count);
        Assert.All(result.Series, s => Assert.False(string.IsNullOrWhiteSpace(s.Month)));
    }

    [Fact]
    public async Task GetOverviewAsync_Should_Return_Insights()
    {
        // Arrange
        var posts = BuildPosts();

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetOverviewAsync();

        // Assert
        Assert.NotEmpty(result.Insights);
    }

    [Fact]
    public async Task GetOverviewAsync_Should_Return_PostsThisMonth_Correctly()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Current month post 1",
                CreatedAt = now.AddDays(-2),
                CityId = 1,
                City = new City { Id = 1, Name = "Madrid", CountryIso2 = "ES" },
                PostTopics = new List<PostTopic>
                {
                    new() { Topic = new Topic { Id = 1, Name = "Housing" } }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Current month post 2",
                CreatedAt = now.AddDays(-5),
                CityId = 2,
                City = new City { Id = 2, Name = "Barcelona", CountryIso2 = "ES" },
                PostTopics = new List<PostTopic>
                {
                    new() { Topic = new Topic { Id = 2, Name = "Food" } }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Previous month post",
                CreatedAt = new DateTime(now.Year, now.Month, 1).AddDays(-2),
                CityId = 3,
                City = new City { Id = 3, Name = "Rome", CountryIso2 = "IT" },
                PostTopics = new List<PostTopic>
                {
                    new() { Topic = new Topic { Id = 3, Name = "Costs" } }
                }
            }
        };

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetOverviewAsync();

        // Assert
        Assert.Equal(2, result.PostsThisMonth);
    }

    [Fact]
    public async Task GetOverviewAsync_Should_Return_DiversityScore_In_Valid_Range()
    {
        // Arrange
        var posts = BuildPosts();

        postRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetOverviewAsync();

        // Assert
        Assert.InRange(result.TopicDiversityScore, 0, 100);
    }

    private static List<Post> BuildPosts()
    {
        var now = DateTime.UtcNow;

        return new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Madrid housing guide",
                CreatedAt = now.AddDays(-10),
                CityId = 1,
                City = new City
                {
                    Id = 1,
                    Name = "Madrid",
                    CountryIso2 = "ES"
                },
                PostTopics = new List<PostTopic>
                {
                    new()
                    {
                        Topic = new Topic
                        {
                            Id = 1,
                            Name = "Housing"
                        }
                    },
                    new()
                    {
                        Topic = new Topic
                        {
                            Id = 2,
                            Name = "Costs"
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Barcelona student budget",
                CreatedAt = now.AddMonths(-1).AddDays(2),
                CityId = 2,
                City = new City
                {
                    Id = 2,
                    Name = "Barcelona",
                    CountryIso2 = "ES"
                },
                PostTopics = new List<PostTopic>
                {
                    new()
                    {
                        Topic = new Topic
                        {
                            Id = 1,
                            Name = "Housing"
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Rome nightlife",
                CreatedAt = now.AddDays(-3),
                CityId = 3,
                City = new City
                {
                    Id = 3,
                    Name = "Rome",
                    CountryIso2 = "IT"
                },
                PostTopics = new List<PostTopic>
                {
                    new()
                    {
                        Topic = new Topic
                        {
                            Id = 3,
                            Name = "Food"
                        }
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Rome housing costs",
                CreatedAt = now.AddDays(-1),
                CityId = 3,
                City = new City
                {
                    Id = 3,
                    Name = "Rome",
                    CountryIso2 = "IT"
                },
                PostTopics = new List<PostTopic>
                {
                    new()
                    {
                        Topic = new Topic
                        {
                            Id = 1,
                            Name = "Housing"
                        }
                    },
                    new()
                    {
                        Topic = new Topic
                        {
                            Id = 2,
                            Name = "Costs"
                        }
                    }
                }
            }
        };
    }
}