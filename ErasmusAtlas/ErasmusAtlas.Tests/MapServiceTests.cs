using ErasmusAtlas.Core.Implementations;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using ErasmusAtlas.Tests.TestHelpers;
using ErasmusAtlas.ViewModels.MapViewModels;

using Moq;

using NetTopologySuite.Geometries;

namespace ErasmusAtlas.Tests.Services;

public class MapServiceTests
{
    private readonly Mock<IRepository<Post, Guid>> postsRepositoryMock;
    private readonly MapService service;

    public MapServiceTests()
    {
        postsRepositoryMock = new Mock<IRepository<Post, Guid>>();
        service = new MapService(postsRepositoryMock.Object);
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Throw_When_Request_Is_Null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.GetPostMapMarkersAsync(null!));
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Return_Only_Posts_With_Location()
    {
        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "With location",
                Body = "Body 1",
                CreatedAt = DateTime.UtcNow,
                CityId = 1,
                Location = CreatePoint(23.3219, 42.6977),
                PostTopics = new List<PostTopic>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Without location",
                Body = "Body 2",
                CreatedAt = DateTime.UtcNow,
                CityId = 1,
                Location = null,
                PostTopics = new List<PostTopic>()
            }
        };

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        var request = new PostMapMarkersRequestViewModel
        {
            CoordinateRoundingPrecision = 4,
            MaxPostsReturnedTotal = 100,
            MaxPostsReturnedPerMarker = 10,
            PostBodyMaxLength = 50
        };

        var result = await service.GetPostMapMarkersAsync(request);

        Assert.Single(result.Items);
        Assert.Single(result.Items[0].Posts);
        Assert.Equal("With location", result.Items[0].Posts[0].Title);
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Filter_By_CityId()
    {
        var posts = BuildPosts();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        var request = new PostMapMarkersRequestViewModel
        {
            CityId = 2,
            CoordinateRoundingPrecision = 4,
            MaxPostsReturnedTotal = 100,
            MaxPostsReturnedPerMarker = 10,
            PostBodyMaxLength = 50
        };

        var result = await service.GetPostMapMarkersAsync(request);

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items.SelectMany(i => i.Posts), p =>
            Assert.Contains(p.Title, new[] { "Madrid housing", "Madrid cafes" }));
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Filter_By_Topic()
    {
        var posts = BuildPosts();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        var request = new PostMapMarkersRequestViewModel
        {
            Topic = "Housing",
            CoordinateRoundingPrecision = 4,
            MaxPostsReturnedTotal = 100,
            MaxPostsReturnedPerMarker = 10,
            PostBodyMaxLength = 50
        };

        var result = await service.GetPostMapMarkersAsync(request);

        var allPosts = result.Items.SelectMany(i => i.Posts).ToList();

        Assert.Equal(2, allPosts.Count);
        Assert.Contains(allPosts, p => p.Title == "Sofia housing");
        Assert.Contains(allPosts, p => p.Title == "Madrid housing");
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Filter_By_Valid_BoundingBox()
    {
        var posts = BuildPosts();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Bounding box around Sofia only
        var request = new PostMapMarkersRequestViewModel
        {
            BoundingBox = "23.0,42.5,23.5,42.9",
            CoordinateRoundingPrecision = 4,
            MaxPostsReturnedTotal = 100,
            MaxPostsReturnedPerMarker = 10,
            PostBodyMaxLength = 50
        };

        var result = await service.GetPostMapMarkersAsync(request);

        var allPosts = result.Items.SelectMany(i => i.Posts).ToList();

        Assert.Single(allPosts);
        Assert.Equal("Sofia housing", allPosts[0].Title);
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Ignore_Invalid_BoundingBox()
    {
        var posts = BuildPosts();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        var request = new PostMapMarkersRequestViewModel
        {
            BoundingBox = "invalid-bbox",
            CoordinateRoundingPrecision = 4,
            MaxPostsReturnedTotal = 100,
            MaxPostsReturnedPerMarker = 10,
            PostBodyMaxLength = 50
        };

        var result = await service.GetPostMapMarkersAsync(request);

        var allPosts = result.Items.SelectMany(i => i.Posts).ToList();

        Assert.Equal(3, allPosts.Count);
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Group_Posts_By_Rounded_Coordinates()
    {
        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Post 1",
                Body = "Body 1",
                CreatedAt = DateTime.UtcNow,
                Location = CreatePoint(23.32191, 42.69771),
                PostTopics = new List<PostTopic>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Post 2",
                Body = "Body 2",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                Location = CreatePoint(23.32194, 42.69774),
                PostTopics = new List<PostTopic>()
            }
        };

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        var request = new PostMapMarkersRequestViewModel
        {
            CoordinateRoundingPrecision = 3,
            MaxPostsReturnedTotal = 100,
            MaxPostsReturnedPerMarker = 10,
            PostBodyMaxLength = 50
        };

        var result = await service.GetPostMapMarkersAsync(request);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Posts.Count);
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Trim_Body_To_Max_Length()
    {
        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Trimmed post",
                Body = "1234567890ABCDE",
                CreatedAt = DateTime.UtcNow,
                Location = CreatePoint(23.3219, 42.6977),
                PostTopics = new List<PostTopic>()
            }
        };

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        var request = new PostMapMarkersRequestViewModel
        {
            CoordinateRoundingPrecision = 4,
            MaxPostsReturnedTotal = 100,
            MaxPostsReturnedPerMarker = 10,
            PostBodyMaxLength = 10
        };

        var result = await service.GetPostMapMarkersAsync(request);

        var post = Assert.Single(result.Items[0].Posts);
        Assert.Equal("1234567890", post.Body);
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Set_Body_Null_When_MaxLength_Is_Zero()
    {
        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "No body",
                Body = "Body text",
                CreatedAt = DateTime.UtcNow,
                Location = CreatePoint(23.3219, 42.6977),
                PostTopics = new List<PostTopic>()
            }
        };

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        var request = new PostMapMarkersRequestViewModel
        {
            CoordinateRoundingPrecision = 4,
            MaxPostsReturnedTotal = 100,
            MaxPostsReturnedPerMarker = 10,
            PostBodyMaxLength = 0
        };

        var result = await service.GetPostMapMarkersAsync(request);

        var post = Assert.Single(result.Items[0].Posts);
        Assert.Null(post.Body);
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Respect_MaxPostsReturnedTotal()
    {
        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Post 1",
                Body = "Body 1",
                CreatedAt = DateTime.UtcNow,
                Location = CreatePoint(23.1, 42.1),
                PostTopics = new List<PostTopic>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Post 2",
                Body = "Body 2",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                Location = CreatePoint(24.1, 43.1),
                PostTopics = new List<PostTopic>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Post 3",
                Body = "Body 3",
                CreatedAt = DateTime.UtcNow.AddMinutes(-2),
                Location = CreatePoint(25.1, 44.1),
                PostTopics = new List<PostTopic>()
            }
        };

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        var request = new PostMapMarkersRequestViewModel
        {
            CoordinateRoundingPrecision = 4,
            MaxPostsReturnedTotal = 2,
            MaxPostsReturnedPerMarker = 10,
            PostBodyMaxLength = 50
        };

        var result = await service.GetPostMapMarkersAsync(request);

        var allPosts = result.Items.SelectMany(i => i.Posts).ToList();

        Assert.Equal(2, allPosts.Count);
        Assert.Contains(allPosts, p => p.Title == "Post 1");
        Assert.Contains(allPosts, p => p.Title == "Post 2");
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Respect_MaxPostsReturnedPerMarker()
    {
        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Post 1",
                Body = "Body 1",
                CreatedAt = DateTime.UtcNow,
                Location = CreatePoint(23.3219, 42.6977),
                PostTopics = new List<PostTopic>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Post 2",
                Body = "Body 2",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                Location = CreatePoint(23.32191, 42.69771),
                PostTopics = new List<PostTopic>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Post 3",
                Body = "Body 3",
                CreatedAt = DateTime.UtcNow.AddMinutes(-2),
                Location = CreatePoint(23.32192, 42.69772),
                PostTopics = new List<PostTopic>()
            }
        };

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        var request = new PostMapMarkersRequestViewModel
        {
            CoordinateRoundingPrecision = 3,
            MaxPostsReturnedTotal = 100,
            MaxPostsReturnedPerMarker = 2,
            PostBodyMaxLength = 50
        };

        var result = await service.GetPostMapMarkersAsync(request);

        Assert.Single(result.Items);
        Assert.Equal(2, result.Items[0].Posts.Count);
    }

    [Fact]
    public async Task GetPostMapMarkersAsync_Should_Clamp_Invalid_Values()
    {
        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Post 1",
                Body = "1234567890",
                CreatedAt = DateTime.UtcNow,
                Location = CreatePoint(23.32191, 42.69771),
                PostTopics = new List<PostTopic>()
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Post 2",
                Body = "abcdefghij",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                Location = CreatePoint(23.32199, 42.69779),
                PostTopics = new List<PostTopic>()
            }
        };

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        var request = new PostMapMarkersRequestViewModel
        {
            CoordinateRoundingPrecision = 1, // should clamp to 2
            MaxPostsReturnedTotal = 0,       // should clamp to 1
            MaxPostsReturnedPerMarker = 0,   // should clamp to 1
            PostBodyMaxLength = 5
        };

        var result = await service.GetPostMapMarkersAsync(request);

        Assert.Single(result.Items);
        Assert.Single(result.Items[0].Posts);
        Assert.Equal(42.70, result.Items[0].Lat, 2);
        Assert.Equal(23.32, result.Items[0].Lng, 2);
        Assert.Equal("12345", result.Items[0].Posts[0].Body);
    }

    private static List<Post> BuildPosts()
    {
        return new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Sofia housing",
                Body = "Housing in Sofia",
                CreatedAt = DateTime.UtcNow,
                CityId = 1,
                Location = CreatePoint(23.3219, 42.6977),
                PostTopics = new List<PostTopic>
                {
                    new() { Topic = new Topic { Id = 1, Name = "Housing" } }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Madrid housing",
                Body = "Housing in Madrid",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1),
                CityId = 2,
                Location = CreatePoint(-3.7038, 40.4168),
                PostTopics = new List<PostTopic>
                {
                    new() { Topic = new Topic { Id = 1, Name = "Housing" } }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Madrid cafes",
                Body = "Food in Madrid",
                CreatedAt = DateTime.UtcNow.AddMinutes(-2),
                CityId = 2,
                Location = CreatePoint(-3.7037, 40.4169),
                PostTopics = new List<PostTopic>
                {
                    new() { Topic = new Topic { Id = 2, Name = "Food" } }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "No location post",
                Body = "Should not appear",
                CreatedAt = DateTime.UtcNow.AddMinutes(-3),
                CityId = 3,
                Location = null,
                PostTopics = new List<PostTopic>()
            }
        };
    }

    private static Point CreatePoint(double lng, double lat)
    {
        return new Point(lng, lat) { SRID = 4326 };
    }
}