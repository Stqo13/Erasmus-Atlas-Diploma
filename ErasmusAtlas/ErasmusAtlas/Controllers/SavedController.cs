using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

using ErasmusAtlas.Core.Interfaces;

namespace ErasmusAtlas.Controllers;

public class SavedController(
    ISavedContentService savedContentService,
    ILogger<SavedController> logger)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Posts()
    {
        string userId = GetCurrentClientId();

        try
        {
            var model = await savedContentService.GetSavedPostsAsync(userId);
            return View(model);
        }
        catch (NullReferenceException ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Projects()
    {
        string userId = GetCurrentClientId();

        try
        {
            var model = await savedContentService.GetSavedProjectsAsync(userId);
            return View(model);
        }
        catch (NullReferenceException ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePost(Guid postId)
    {
        string userId = GetCurrentClientId();

        try
        {
            await savedContentService.SavePostAsync(postId, userId);
            return RedirectToAction("Details", "Posts", new { id = postId });
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnsavePost(Guid postId)
    {
        string userId = GetCurrentClientId();

        try
        {
            await savedContentService.UnsavePostAsync(postId, userId);
            return RedirectToAction("Details", "Posts", new { id = postId });
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProject(Guid projectId)
    {
        string userId = GetCurrentClientId();

        try
        {
            await savedContentService.SaveProjectAsync(projectId, userId);
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnsaveProject(Guid projectId)
    {
        string userId = GetCurrentClientId();

        try
        {
            await savedContentService.UnsaveProjectAsync(projectId, userId);
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }
        catch (Exception ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }

    private string GetCurrentClientId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
