using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.Infrastructure;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using ErasmusAtlas.ViewModels.PostViewModels;
using ErasmusAtlas.ViewModels.ProjectViewModels;
using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Core.Implementations;

public class SavedContentService(
    IRepository<Post, Guid> postRepository,
    IRepository<Project, Guid> projectRepository,
    ErasmusAtlasDbContext db)
    : ISavedContentService
{
    public async Task<IEnumerable<PostInfoViewModel>> GetSavedPostsAsync(string userId)
    {
        return await db.SavedPosts
            .Where(sp => sp.UserId == userId)
            .OrderByDescending(sp => sp.SavedAt)
            .Select(sp => new PostInfoViewModel
            {
                Id = sp.Post.Id,
                Title = sp.Post.Title,
                CreatedOn = sp.Post.CreatedAt,
                City = sp.Post.City != null ? sp.Post.City.Name : "Unknown",
                Topics = sp.Post.PostTopics
                    .Select(pt => pt.Topic.Name)
                    .OrderBy(t => t)
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<ProjectInfoViewModel>> GetSavedProjectsAsync(string userId)
    {
        return await db.SavedProjects
            .Where(sp => sp.UserId == userId)
            .OrderByDescending(sp => sp.SavedAt)
            .Select(sp => new ProjectInfoViewModel
            {
                Id = sp.Project.Id,
                Title = sp.Project.Title,
                DescriptionPreview = sp.Project.Description.Length > 220
                    ? sp.Project.Description.Substring(0, 220) + "..."
                    : sp.Project.Description,
                City = sp.Project.City != null ? sp.Project.City.Name : "Unknown",
                Institution = sp.Project.Institution != null ? sp.Project.Institution.Name : "Unknown",
                ProjectType = sp.Project.ProjectType.Name,
                Tags = sp.Project.ProjectTags
                    .Select(pt => pt.Tag.Name)
                    .OrderBy(t => t)
                    .ToList(),
                CreatedOn = sp.Project.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> IsPostSavedAsync(Guid postId, string userId)
    {
        return await db.SavedPosts
            .AnyAsync(sp => sp.UserId == userId && sp.PostId == postId);
    }

    public async Task<bool> IsProjectSavedAsync(Guid projectId, string userId)
    {
        return await db.SavedProjects
            .AnyAsync(sp => sp.UserId == userId && sp.ProjectId == projectId);
    }

    public async Task<bool> SavePostAsync(Guid postId, string userId)
    {
        bool postExists = await postRepository
            .GetAllAttached()
            .AnyAsync(p => p.Id == postId);

        if (!postExists)
        {
            return false;
        }

        bool alreadySaved = await db.SavedPosts
            .AnyAsync(sp => sp.UserId == userId && sp.PostId == postId);

        if (alreadySaved)
        {
            return true;
        }

        db.SavedPosts.Add(new SavedPost
        {
            UserId = userId,
            PostId = postId,
            SavedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SaveProjectAsync(Guid projectId, string userId)
    {
        bool projectExists = await projectRepository
            .GetAllAttached()
            .AnyAsync(p => p.Id == projectId);

        if (!projectExists)
        {
            return false;
        }

        bool alreadySaved = await db.SavedProjects
            .AnyAsync(sp => sp.UserId == userId && sp.ProjectId == projectId);

        if (alreadySaved)
        {
            return true;
        }

        db.SavedProjects.Add(new SavedProject
        {
            UserId = userId,
            ProjectId = projectId,
            SavedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnsavePostAsync(Guid postId, string userId)
    {
        var savedPost = await db.SavedPosts
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);

        if (savedPost is null)
        {
            return false;
        }

        db.SavedPosts.Remove(savedPost);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnsaveProjectAsync(Guid projectId, string userId)
    {
        var savedProject = await db.SavedProjects
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.ProjectId == projectId);

        if (savedProject is null)
        {
            return false;
        }

        db.SavedProjects.Remove(savedProject);
        await db.SaveChangesAsync();
        return true;
    }
}