using ErasmusAtlas.Core.Implementations;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using ErasmusAtlas.Tests.TestHelpers;
using ErasmusAtlas.ViewModels.PostViewModels;

using Moq;

namespace ErasmusAtlas.Tests.Services;

public class PostServiceTests
{
    private readonly Mock<IRepository<Post, Guid>> postsRepositoryMock;
    private readonly Mock<IRepository<City, int>> citiesRepositoryMock;
    private readonly Mock<IRepository<Topic, int>> topicsRepositoryMock;
    private readonly Mock<IRepository<SavedPost, object>> savedPostsRepositoryMock;

    private readonly PostService service;

    public PostServiceTests()
    {
        postsRepositoryMock = new Mock<IRepository<Post, Guid>>();
        citiesRepositoryMock = new Mock<IRepository<City, int>>();
        topicsRepositoryMock = new Mock<IRepository<Topic, int>>();
        savedPostsRepositoryMock = new Mock<IRepository<SavedPost, object>>();

        service = new PostService(
            postsRepositoryMock.Object,
            citiesRepositoryMock.Object,
            topicsRepositoryMock.Object,
            savedPostsRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Should_Add_Post_With_Distinct_Topics_And_Explicit_Location()
    {
        // Arrange
        Post? addedPost = null;

        postsRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Post>()))
            .Callback<Post>(p => addedPost = p)
            .Returns(Task.CompletedTask);

        var model = new CreatePostViewModel
        {
            Title = "Housing in Madrid",
            Body = "Useful guide",
            CityId = 1,
            Longitude = -3.7038,
            Latitude = 40.4168,
            TopicIds = new List<int> { 1, 2, 2 }
        };

        // Act
        await service.CreateAsync(model, "user-1");

        // Assert
        Assert.NotNull(addedPost);
        Assert.Equal("Housing in Madrid", addedPost!.Title);
        Assert.Equal("Useful guide", addedPost.Body);
        Assert.Equal(1, addedPost.CityId);
        Assert.Equal("user-1", addedPost.UserId);
        Assert.Equal("Published", addedPost.Status);
        Assert.NotNull(addedPost.Location);
        Assert.Equal(-3.7038, addedPost.Location!.X, 4);
        Assert.Equal(40.4168, addedPost.Location.Y, 4);
        Assert.Equal(4326, addedPost.Location.SRID);
        Assert.Equal(2, addedPost.PostTopics.Count);
        Assert.Contains(addedPost.PostTopics, pt => pt.TopicId == 1);
        Assert.Contains(addedPost.PostTopics, pt => pt.TopicId == 2);
    }

    [Fact]
    public async Task CreateAsync_Should_Resolve_Location_From_City_When_Coordinates_Are_Missing()
    {
        // Arrange
        Post? addedPost = null;

        citiesRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<City>
            {
                new()
                {
                    Id = 5,
                    Name = "Prague",
                    Latitude = 50.0755,
                    Longitude = 14.4378
                }
            }.AsAsyncQueryable());

        postsRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Post>()))
            .Callback<Post>(p => addedPost = p)
            .Returns(Task.CompletedTask);

        var model = new CreatePostViewModel
        {
            Title = "Life in Prague",
            Body = "Beautiful city",
            CityId = 5,
            TopicIds = new List<int> { 1 }
        };

        // Act
        await service.CreateAsync(model, "user-1");

        // Assert
        Assert.NotNull(addedPost);
        Assert.NotNull(addedPost!.Location);
        Assert.Equal(14.4378, addedPost.Location!.X, 4);
        Assert.Equal(50.0755, addedPost.Location.Y, 4);
    }

    [Fact]
    public async Task DeleteAsync_Should_Return_False_When_Post_Does_Not_Belong_To_User()
    {
        // Arrange
        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>().AsAsyncQueryable());

        // Act
        var result = await service.DeleteAsync(Guid.NewGuid(), "user-1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_Should_Call_DeleteAsync_When_Post_Exists_For_User()
    {
        // Arrange
        var postId = Guid.NewGuid();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>
            {
                new()
                {
                    Id = postId,
                    UserId = "user-1",
                    PostTopics = new List<PostTopic>()
                }
            }.AsAsyncQueryable());

        postsRepositoryMock
            .Setup(r => r.DeleteAsync(postId))
            .ReturnsAsync(true);

        // Act
        var result = await service.DeleteAsync(postId, "user-1");

        // Assert
        Assert.True(result);
        postsRepositoryMock.Verify(r => r.DeleteAsync(postId), Times.Once);
    }

    [Fact]
    public async Task EditAsync_Should_Return_False_When_Post_Is_Not_Found()
    {
        // Arrange
        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>().AsAsyncQueryable());

        var model = new EditPostViewModel
        {
            Title = "Updated",
            Body = "Updated body",
            CityId = 1,
            TopicIds = new List<int> { 1 }
        };

        // Act
        var result = await service.EditAsync(Guid.NewGuid(), model, "user-1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EditAsync_Should_Update_Post_And_Topics()
    {
        // Arrange
        var postId = Guid.NewGuid();

        var existingPost = new Post
        {
            Id = postId,
            Title = "Old title",
            Body = "Old body",
            CityId = 1,
            UserId = "user-1",
            PostTopics = new List<PostTopic>
            {
                new() { PostId = postId, TopicId = 1 }
            }
        };

        citiesRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<City>
            {
                new()
                {
                    Id = 2,
                    Name = "Berlin",
                    Latitude = 52.5200,
                    Longitude = 13.4050
                }
            }.AsAsyncQueryable());

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post> { existingPost }.AsAsyncQueryable());

        postsRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Post>()))
            .ReturnsAsync(true);

        var model = new EditPostViewModel
        {
            Title = "New title",
            Body = "New body",
            CityId = 2,
            TopicIds = new List<int> { 2, 3, 3 }
        };

        // Act
        var result = await service.EditAsync(postId, model, "user-1");

        // Assert
        Assert.True(result);
        Assert.Equal("New title", existingPost.Title);
        Assert.Equal("New body", existingPost.Body);
        Assert.Equal(2, existingPost.CityId);
        Assert.NotNull(existingPost.Location);
        Assert.Equal(13.4050, existingPost.Location!.X, 4);
        Assert.Equal(52.5200, existingPost.Location.Y, 4);
        Assert.Equal(2, existingPost.PostTopics.Count);
        Assert.Contains(existingPost.PostTopics, pt => pt.TopicId == 2);
        Assert.Contains(existingPost.PostTopics, pt => pt.TopicId == 3);
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Return_All_When_No_Filters()
    {
        // Arrange
        var posts = BuildPosts();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetAllFilteredAsync(null, null, 1, 10);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Posts.Count);
        Assert.Equal(1, result.CurrentPage);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Filter_By_City()
    {
        // Arrange
        var posts = BuildPosts();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetAllFilteredAsync("Madrid", null, 1, 10);

        // Assert
        Assert.Equal(2, result.Posts.Count);
        Assert.All(result.Posts, p => Assert.Equal("Madrid", p.City));
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Filter_By_Topic()
    {
        // Arrange
        var posts = BuildPosts();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetAllFilteredAsync(null, "Housing", 1, 10);

        // Assert
        Assert.Equal(2, result.Posts.Count);
        Assert.All(result.Posts, p => Assert.Contains("Housing", p.Topics));
    }

    [Fact]
    public async Task GetAllFilteredAsync_Should_Apply_Pagination()
    {
        // Arrange
        var posts = BuildPosts();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(posts.AsAsyncQueryable());

        // Act
        var result = await service.GetAllFilteredAsync(null, null, 2, 1);

        // Assert
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.CurrentPage);
        Assert.Single(result.Posts);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Details_With_IsSaved_True()
    {
        // Arrange
        var postId = Guid.NewGuid();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>
            {
                new()
                {
                    Id = postId,
                    Title = "Madrid housing",
                    Body = "Body text",
                    CreatedAt = DateTime.UtcNow,
                    City = new City { Id = 1, Name = "Madrid" },
                    UserId = "author-1",
                    User = new ErasmusUser { UserName = "authorUser" },
                    Location = null,
                    PostTopics = new List<PostTopic>
                    {
                        new() { Topic = new Topic { Id = 1, Name = "Housing" } }
                    }
                }
            }.AsAsyncQueryable());

        savedPostsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<SavedPost>
            {
                new() { UserId = "viewer-1", PostId = postId }
            }.AsAsyncQueryable());

        // Act
        var result = await service.GetByIdAsync(postId, "viewer-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(postId, result!.Id);
        Assert.Equal("Madrid housing", result.Title);
        Assert.Equal("Madrid", result.City);
        Assert.Equal("author-1", result.AuthorId);
        Assert.Equal("authorUser", result.AuthorName);
        Assert.True(result.IsSaved);
        Assert.False(result.CanEdit);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Set_CanEdit_True_When_User_Is_Author()
    {
        // Arrange
        var postId = Guid.NewGuid();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>
            {
                new()
                {
                    Id = postId,
                    Title = "Own post",
                    Body = "Body text",
                    CreatedAt = DateTime.UtcNow,
                    City = new City { Id = 1, Name = "Sofia" },
                    UserId = "user-1",
                    User = new ErasmusUser { UserName = "user1" },
                    PostTopics = new List<PostTopic>()
                }
            }.AsAsyncQueryable());

        savedPostsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<SavedPost>().AsAsyncQueryable());

        // Act
        var result = await service.GetByIdAsync(postId, "user-1");

        // Assert
        Assert.NotNull(result);
        Assert.True(result!.CanEdit);
        Assert.False(result.IsSaved);
    }

    [Fact]
    public async Task GetCitiesAsync_Should_Return_Cities_Ordered_By_Name()
    {
        // Arrange
        citiesRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<City>
            {
                new() { Id = 2, Name = "Sofia" },
                new() { Id = 1, Name = "Berlin" }
            }.AsAsyncQueryable());

        // Act
        var result = (await service.GetCitiesAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Berlin", result[0].Name);
        Assert.Equal("Sofia", result[1].Name);
    }

    [Fact]
    public async Task GetDeleteModelAsync_Should_Return_Mapped_Model()
    {
        // Arrange
        var postId = Guid.NewGuid();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>
            {
                new()
                {
                    Id = postId,
                    Title = "Delete me",
                    UserId = "user-1",
                    City = new City { Id = 1, Name = "Rome" },
                    PostTopics = new List<PostTopic>
                    {
                        new() { Topic = new Topic { Id = 1, Name = "Food" } }
                    }
                }
            }.AsAsyncQueryable());

        // Act
        var result = await service.GetDeleteModelAsync(postId, "user-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Delete me", result!.Title);
        Assert.Equal("Rome", result.City);
        Assert.Contains("Food", result.Topics);
    }

    [Fact]
    public async Task GetEditModelAsync_Should_Return_Mapped_Model()
    {
        // Arrange
        var postId = Guid.NewGuid();

        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>
            {
                new()
                {
                    Id = postId,
                    Title = "Edit me",
                    Body = "Body",
                    UserId = "user-1",
                    CityId = 2,
                    Location = new NetTopologySuite.Geometries.Point(23.3219, 42.6977) { SRID = 4326 },
                    PostTopics = new List<PostTopic>
                    {
                        new() { TopicId = 1 },
                        new() { TopicId = 3 }
                    }
                }
            }.AsAsyncQueryable());

        // Act
        var result = await service.GetEditModelAsync(postId, "user-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Edit me", result!.Title);
        Assert.Equal("Body", result.Body);
        Assert.Equal(2, result.CityId);
        Assert.Equal(42.6977, result.Latitude!.Value, 4);
        Assert.Equal(23.3219, result.Longitude!.Value, 4);
        Assert.Equal(2, result.TopicIds.Count);
        Assert.Contains(1, result.TopicIds);
        Assert.Contains(3, result.TopicIds);
    }

    [Fact]
    public async Task GetMineAsync_Should_Return_Only_Users_Posts()
    {
        // Arrange
        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "My first post",
                    UserId = "user-1",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    City = new City { Id = 1, Name = "Madrid" },
                    PostTopics = new List<PostTopic>()
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Someone else's post",
                    UserId = "user-2",
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    City = new City { Id = 2, Name = "Berlin" },
                    PostTopics = new List<PostTopic>()
                }
            }.AsAsyncQueryable());

        // Act
        var result = (await service.GetMineAsync("user-1")).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("My first post", result[0].Title);
    }

    [Fact]
    public async Task GetTopicsAsync_Should_Return_Topics_Ordered_By_Name()
    {
        // Arrange
        topicsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Topic>
            {
                new() { Id = 2, Name = "Nightlife" },
                new() { Id = 1, Name = "Food" }
            }.AsAsyncQueryable());

        // Act
        var result = (await service.GetTopicsAsync()).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Food", result[0].Name);
        Assert.Equal("Nightlife", result[1].Name);
    }

    [Fact]
    public async Task GetLatestAsync_Should_Return_Latest_Posts_First()
    {
        // Arrange
        postsRepositoryMock
            .Setup(r => r.GetAllAttached())
            .Returns(new List<Post>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Older",
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    City = new City { Id = 1, Name = "Madrid" },
                    PostTopics = new List<PostTopic>()
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Newest",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    City = new City { Id = 2, Name = "Berlin" },
                    PostTopics = new List<PostTopic>()
                }
            }.AsAsyncQueryable());

        // Act
        var result = (await service.GetLatestAsync(1)).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Newest", result[0].Title);
    }

    private static List<Post> BuildPosts()
    {
        return new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Finding a flat in Madrid",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                City = new City { Id = 1, Name = "Madrid" },
                PostTopics = new List<PostTopic>
                {
                    new() { Topic = new Topic { Id = 1, Name = "Housing" } },
                    new() { Topic = new Topic { Id = 2, Name = "Costs" } }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Student life in Sofia",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                City = new City { Id = 2, Name = "Sofia" },
                PostTopics = new List<PostTopic>
                {
                    new() { Topic = new Topic { Id = 1, Name = "Housing" } }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Best cafes in Madrid",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                City = new City { Id = 1, Name = "Madrid" },
                PostTopics = new List<PostTopic>
                {
                    new() { Topic = new Topic { Id = 3, Name = "Food" } }
                }
            }
        };
    }
}