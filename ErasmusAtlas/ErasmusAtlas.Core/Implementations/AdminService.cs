using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.Infrastructure;
using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.ViewModels.AdminViewModels;
using ErasmusAtlas.ViewModels.PostViewModels;
using ErasmusAtlas.ViewModels.ProjectViewModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Core.Implementations;

public class AdminService(
    ErasmusAtlasDbContext context,
    UserManager<ErasmusUser> userManager,
    RoleManager<IdentityRole> roleManager)
    : IAdminService
{
    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        var latestUsersRaw = await context.Users
            .OrderByDescending(u => u.Id)
            .Take(5)
            .ToListAsync();

        var latestUsers = new List<AdminUserListItemViewModel>();

        foreach (var user in latestUsersRaw)
        {
            var roles = await userManager.GetRolesAsync(user);

            latestUsers.Add(new AdminUserListItemViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName ?? string.Empty : user.DisplayName!,
                Roles = roles.ToList()
            });
        }

        return new AdminDashboardViewModel
        {
            UsersCount = await context.Users.CountAsync(),
            PostsCount = await context.Posts.CountAsync(),
            ProjectsCount = await context.Projects.CountAsync(),
            SavedPostsCount = await context.SavedPosts.CountAsync(),
            SavedProjectsCount = await context.SavedProjects.CountAsync(),

            LatestPosts = await context.Posts
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
                .ToListAsync(),

            LatestProjects = await context.Projects
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new ProjectInfoViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    DescriptionPreview = p.Description.Length > 140
                        ? p.Description.Substring(0, 140) + "..."
                        : p.Description,
                    City = p.City != null ? p.City.Name : "Unknown",
                    Institution = p.Institution != null ? p.Institution.Name : "Unknown",
                    ProjectType = p.ProjectType.Name,
                    Tags = p.ProjectTags
                        .Select(pt => pt.Tag.Name)
                        .OrderBy(t => t)
                        .ToList(),
                    CreatedOn = p.CreatedAt
                })
                .ToListAsync(),

            LatestUsers = latestUsers
        };
    }

    public async Task<AdminUsersPageViewModel> GetUsersAsync()
    {
        var users = await context.Users
            .OrderBy(u => u.UserName)
            .ToListAsync();

        var result = new List<AdminUserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            result.Add(new AdminUserListItemViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                DisplayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName ?? string.Empty : user.DisplayName!,
                Roles = roles.ToList()
            });
        }

        return new AdminUsersPageViewModel
        {
            Users = result
        };
    }

    public async Task<AdminEditUserViewModel?> GetEditUserAsync(string userId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var allRoles = await roleManager.Roles
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync();

        return new AdminEditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            CurrentRoles = currentRoles.ToList(),
            SelectedRoles = currentRoles.ToList(),
            AllRoles = allRoles
        };
    }

    public async Task<bool> EditUserAsync(AdminEditUserViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.Id);

        if (user == null)
        {
            return false;
        }

        user.Email = model.Email;
        user.UserName = model.UserName;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.DisplayName = model.DisplayName;
        user.Bio = model.Bio;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return false;
        }

        var currentRoles = await userManager.GetRolesAsync(user);

        var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();
        var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();

        if (rolesToRemove.Any())
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return false;
            }
        }

        if (rolesToAdd.Any())
        {
            var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<AdminPostsPageViewModel> GetPostsAsync()
    {
        var posts = await context.Posts
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

        return new AdminPostsPageViewModel
        {
            Posts = posts
        };
    }

    public async Task<AdminProjectsPageViewModel> GetProjectsAsync()
    {
        var projects = await context.Projects
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectInfoViewModel
            {
                Id = p.Id,
                Title = p.Title,
                DescriptionPreview = p.Description.Length > 140
                    ? p.Description.Substring(0, 140) + "..."
                    : p.Description,
                City = p.City != null ? p.City.Name : "Unknown",
                Institution = p.Institution != null ? p.Institution.Name : "Unknown",
                ProjectType = p.ProjectType.Name,
                Tags = p.ProjectTags
                    .Select(pt => pt.Tag.Name)
                    .OrderBy(t => t)
                    .ToList(),
                CreatedOn = p.CreatedAt
            })
            .ToListAsync();

        return new AdminProjectsPageViewModel
        {
            Projects = projects
        };
    }

    public async Task<bool> DeletePostAsync(Guid postId)
    {
        var post = await context.Posts.FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
        {
            return false;
        }

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProjectAsync(Guid projectId)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return false;
        }

        context.Projects.Remove(project);
        await context.SaveChangesAsync();
        return true;
    }
}
