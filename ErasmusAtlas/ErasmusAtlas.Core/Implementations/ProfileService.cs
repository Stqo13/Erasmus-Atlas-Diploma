using Microsoft.EntityFrameworkCore;

using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.Infrastructure;
using ErasmusAtlas.ViewModels.AccountViewModels;
using ErasmusAtlas.ViewModels.PostViewModels;
using ErasmusAtlas.ViewModels.ProjectViewModels;

namespace ErasmusAtlas.Core.Implementations;

public class ProfileService(ErasmusAtlasDbContext db) : IProfileService
{
    public async Task<UserProfileViewModel?> GetProfileAsync(string userId, string? currentUserId = null)
    {
        var model = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileViewModel
            {
                UserId = u.Id,
                UserName = u.UserName!,
                DisplayName = string.IsNullOrWhiteSpace(u.DisplayName) ? u.UserName! : u.DisplayName!,
                Bio = u.Bio,
                PostsCount = u.Posts.Count,
                RecentPosts = u.Posts
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
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
                    .ToList(),
                SavedProjectsPreview = u.SavedProjects
                    .OrderByDescending(sp => sp.SavedAt)
                    .Take(5)
                    .Select(sp => new ProjectInfoViewModel
                    {
                        Id = sp.Project.Id,
                        Title = sp.Project.Title,
                        DescriptionPreview = sp.Project.Description.Length > 140
                            ? sp.Project.Description.Substring(0, 140) + "..."
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
                    .ToList(),
                IsOwner = currentUserId != null && u.Id == currentUserId
            })
            .FirstOrDefaultAsync();

        if (model == null)
        {
            return null;
        }

        model.SavedPostsCount = await db.SavedPosts.CountAsync(sp => sp.UserId == userId);
        model.SavedProjectsCount = await db.SavedProjects.CountAsync(sp => sp.UserId == userId);

        return model;
    }

    public async Task<EditProfileViewModel?> GetEditModelAsync(string userId)
    {
        return await db.Users
            .Where(u => u.Id == userId)
            .Select(u => new EditProfileViewModel
            {
                DisplayName = u.DisplayName,
                Bio = u.Bio
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> EditAsync(string userId, EditProfileViewModel model)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return false;
        }

        user.DisplayName = model.DisplayName;
        user.Bio = model.Bio;

        await db.SaveChangesAsync();
        return true;
    }
}
