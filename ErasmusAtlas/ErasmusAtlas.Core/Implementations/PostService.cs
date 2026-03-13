using Microsoft.EntityFrameworkCore;

using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.ViewModels.PostViewModels;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using NetTopologySuite.Geometries;
using ErasmusAtlas.ViewModels.SharedViewModels;

namespace ErasmusAtlas.Core.Implementations;

public class PostService(
    IRepository<Post, Guid> postsRepository,
    IRepository<City, int> citiesRepository,
    IRepository<Topic, int> topicsRepository)
    : IPostService
{
    public async Task CreateAsync(CreatePostViewModel model, string userId)
    {
        var topicIds = model.TopicIds
            .Distinct()
            .ToList();

        var post = new Post
        {
            Title = model.Title,
            Body = model.Body,
            CityId = model.CityId,
            UserId = userId,
            Status = "Published",
            CreatedAt = DateTime.UtcNow,
            Location = CreatePoint(model.Longitude, model.Latitude)
        };

        foreach (var topicId in topicIds)
        {
            post.PostTopics.Add(new PostTopic
            {
                TopicId = topicId
            });
        }

        //SaveChanges is in the repo pattern AddAsync()
        await postsRepository.AddAsync(post);
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var post = await postsRepository
            .GetAllAttached()
            .Include(p => p.PostTopics)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (post == null)
        {
            return false;
        }

        return await postsRepository.DeleteAsync(id);
    }

    public async Task<bool> EditAsync(Guid id, EditPostViewModel model, string userId)
    {
        var post = await postsRepository
            .GetAllAttached()
            .Include(p => p.PostTopics)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        if (post is null)
        {
            return false;
        }

        post.Title = model.Title;
        post.Body = model.Body;
        post.CityId = model.CityId;
        post.Location = CreatePoint(model.Longitude, model.Latitude);

        post.PostTopics.Clear();

        foreach (var topicId in model.TopicIds.Distinct())
        {
            post.PostTopics.Add(new PostTopic
            {
                PostId = post.Id,
                TopicId = topicId
            });
        }

        return await postsRepository.UpdateAsync(post);
    }

    public async Task<IEnumerable<PostInfoViewModel>> GetAllFilteredAsync(string? city, string? topic)
    {
        var query = postsRepository
            .GetAllAttached();

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(p => p.City != null && p.City.Name == city);
        }

        if (!string.IsNullOrWhiteSpace(topic))
        {
            query = query.Where(p => p.PostTopics.Any(pt => pt.Topic.Name == topic));
        }

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostInfoViewModel
            {
                Id = p.Id,
                Title = p.Title,
                CreatedOn = p.CreatedAt,
                City = p.City != null ? p.City.Name : "Unknown",
                Topics = p.PostTopics
                    .Select(pt => pt.Topic.Name)
                    .OrderBy(t => t)
                    .ToList()
            })
            .ToListAsync();

        if (posts is null)
        {
            throw new NullReferenceException("No posts found!");
        }

        return posts;
    }

    public async Task<PostDetailsViewModel?> GetByIdAsync(Guid id, string? currentUserId)
    {
        var post = await postsRepository
            .GetAllAttached()
            .Where(p => p.Id == id)
            .Select(p => new PostDetailsViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                CreatedOn = p.CreatedAt,
                City = p.City != null ? p.City.Name : "Unknown",
                AuthorId = p.UserId,
                AuthorName = p.User.UserName!,
                Topics = p.PostTopics
                    .Select(pt => pt.Topic.Name)
                    .OrderBy(t => t)
                    .ToList(),
                Latitude = p.Location != null ? p.Location.Y : null,
                Longitude = p.Location != null ? p.Location.X : null,
                CanEdit = currentUserId != null && p.UserId == currentUserId
            })
            .FirstOrDefaultAsync();

        if (post is null)
        {
            throw new NullReferenceException("Unable to find this post!");
        }

        return post;
    }

    public async Task<IEnumerable<CityLookupViewModel>> GetCitiesAsync()
    {
        var cities = await citiesRepository
            .GetAllAttached()
            .OrderBy(c => c.Name)
            .Select(c => new CityLookupViewModel
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();

        if (cities is null)
        {
            throw new NullReferenceException("No cities found!");
        }

        return cities;
    }

    public async Task<DeletePostViewModel?> GetDeleteModelAsync(Guid id, string userId)
    {
        var post = await postsRepository
            .GetAllAttached()
            .Where(p => p.Id == id && p.UserId == userId)
            .Select(p => new DeletePostViewModel
            {
                Id = p.Id,
                Title = p.Title,
                City = p.City != null ? p.City.Name : "Unknown",
                Topics = p.PostTopics
                    .Select(pt => pt.Topic.Name)
                    .OrderBy(t => t)
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (post is null)
        {
            throw new NullReferenceException("Unable to find post!");
        }

        return post;
    }

    public async Task<EditPostViewModel?> GetEditModelAsync(Guid id, string userId)
    {
        var post = await postsRepository
            .GetAllAttached()
            .Where(p => p.Id == id && p.UserId == userId)
            .Select(p => new EditPostViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Body = p.Body,
                CityId = p.CityId,
                TopicIds = p.PostTopics
                    .Select(pt => pt.TopicId)
                    .ToList(),
                Latitude = p.Location != null ? p.Location.Y : null,
                Longitude = p.Location != null ? p.Location.X : null
            })
            .FirstOrDefaultAsync();

        if (post is null)
        {
            throw new NullReferenceException("Unable to find post!");
        }

        return post;
    }

    public async Task<IEnumerable<PostInfoViewModel>> GetMineAsync(string userId)
    {
        var posts = await postsRepository
            .GetAllAttached()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostInfoViewModel
            {
                Id = p.Id,
                Title = p.Title,
                CreatedOn = p.CreatedAt,
                City = p.City != null ? p.City.Name : "Unknown",
                Topics = p.PostTopics
                    .Select(pt => pt.Topic.Name)
                    .OrderBy(t => t)
                    .ToList()
            })
            .ToListAsync();

        if (posts is null)
        {
            throw new NullReferenceException("You don't have any posts published!");
        }

        return posts;
    }

    public async Task<IEnumerable<TopicLookupViewModel>> GetTopicsAsync()
    {
        var topics = await topicsRepository
            .GetAllAttached()
            .OrderBy(t => t.Name)
            .Select(t => new TopicLookupViewModel
            {
                Id = t.Id,
                Name = t.Name
            })
            .ToListAsync();

        if (topics is null)
        {
            throw new NullReferenceException("No topics found!");
        }

        return topics;
    }

    private static Point? CreatePoint(double? longitude, double? latitude)
    {
        if (!longitude.HasValue || !latitude.HasValue)
        {
            return null;
        }

        return new Point(longitude.Value, latitude.Value)
        {
            SRID = 4326
        };
    }
}
