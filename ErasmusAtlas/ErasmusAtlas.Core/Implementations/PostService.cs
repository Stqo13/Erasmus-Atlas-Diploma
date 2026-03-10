//using Microsoft.EntityFrameworkCore;

//using ErasmusAtlas.Core.Interfaces;
//using ErasmusAtlas.Infrastructure.Models;
//using ErasmusAtlas.ViewModels.PostViewModels;
//using ErasmusAtlas.Infrastructure.Repository.Interfaces;

//namespace ErasmusAtlas.Core.Implementations;

//public class PostService(
//    IRepository<Post, Guid> postsRepository) 
//    : IPostService
//{
//    public Task CreateAsync(CreatePostViewModel model, string userId)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<bool> DeleteAsync(Guid id, string userId)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<bool> EditAsync(Guid id, EditPostViewModel model, string userId)
//    {
//        throw new NotImplementedException();
//    }

//    public async Task<IEnumerable<PostInfoViewModel>> GetAllFilteredAsync(string? city, string? topic)
//    {
//        var query = postsRepository
//            .GetAllAttached();

//        if (query is null)
//        {
//            throw new NullReferenceException("No entities found!");
//        }

//        if (!string.IsNullOrWhiteSpace(city))
//        {
//            query = query
//                .Include(q => q.City)
//                .Where(p => p.City != null && p.City.Name == city);
//        }

//        if (!string.IsNullOrWhiteSpace(topic))
//        {
//            query = query
//                .Where(q => q.Topic == topic);
//        }

//        var posts = await query
//            .OrderByDescending(p => p.CreatedAt)
//            .Select(p => new PostInfoViewModel
//            {
//                Id = p.Id,
//                Title = p.Title,
//                Topic = p.Topic,
//                CreatedOn = p.CreatedAt,
//                City = p.City != null ? p.City.Name : "Unknown"
//            })
//            .ToListAsync();

//        return posts;
//    }

//    public async Task<PostDetailsViewModel?> GetByIdAsync(Guid id)
//    {
//        throw new NotImplementedException();

//        //var posts = await postsRepository
//        //    .GetAllAttached()
//        //    .Include()
//    }

//    public Task<CreatePostViewModel> GetCreateModelAsync()
//    {
//        throw new NotImplementedException();
//    }

//    public Task<DeletePostViewModel?> GetDeleteModelAsync(Guid id, string userId)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<EditPostViewModel?> GetEditModelAsync(Guid id, string userId)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<IEnumerable<PostInfoViewModel>> GetMineAsync(string userId)
//    {
//        throw new NotImplementedException();
//    }
//}
