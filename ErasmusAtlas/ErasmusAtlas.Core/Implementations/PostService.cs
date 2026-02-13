using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.ViewModels.PostViewModels;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Core.Implementations;

public class PostService(
    IRepository<Post, Guid> postsRepository) 
    : IPostService
{
    public async Task<IEnumerable<PostInfoViewModel>> GetAllFilteredAsync(string? city, string? topic)
    {
        var query = postsRepository
            .GetAllAttached();

        if (query is null)
        {
            throw new NullReferenceException("No entities found!");
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query
                .Include(q => q.City)
                .Where(p => p.City != null && p.City.Name == city);
        }

        if (!string.IsNullOrWhiteSpace(topic))
        {
            query = query
                .Where(q => q.Topic == topic);
        }

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostInfoViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Topic = p.Topic,
                CreatedOn = p.CreatedAt,
                City = p.City != null ? p.City.Name : "Unkown"
            })
            .ToListAsync();

        return posts;
    }

    public async Task<PostDetailsViewModel> GetByIdAsync(Guid id)
    {
        var posts = await postsRepository
            .GetAllAttached()
            .Include()
    }
}
