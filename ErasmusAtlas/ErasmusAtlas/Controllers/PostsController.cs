using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.ViewModels.PostViewModels;

namespace ErasmusAtlas.Controllers;

public class PostsController(
    IPostService postService,
    ILogger<PostsController> logger)
    : Controller
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index(string? city, string? topic, int page = 1, int pageSize = 12)
    {
        var models = await postService.GetAllFilteredAsync(city, topic, page, pageSize);
        return View(models);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Details(Guid id)
    {
        var userId = GetCurrentClientId();

        try
        {
            var post = await postService.GetByIdAsync(id, userId);

            return View(post);
        }
        catch (NullReferenceException ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Create()
    {
        ViewBag.Cities = await postService.GetCitiesAsync();
        ViewBag.Topics = await postService.GetTopicsAsync();

        var model = new CreatePostViewModel();

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Cities = await postService.GetCitiesAsync();
            ViewBag.Topics = await postService.GetTopicsAsync();
            return View(model);
        }

        var userId = GetCurrentClientId();

        await postService.CreateAsync(model, userId);

        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        var userId = GetCurrentClientId();

        try
        {
            var model = await postService.GetEditModelAsync(id, userId);

            ViewBag.Cities = await postService.GetCitiesAsync();
            ViewBag.Topics = await postService.GetTopicsAsync();

            return View(model);
        }
        catch (NullReferenceException ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditPostViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Cities = await postService.GetCitiesAsync();
            ViewBag.Topics = await postService.GetTopicsAsync();
            return View(model);
        }

        var userId = GetCurrentClientId();
        var success = await postService.EditAsync(id, model, userId);

        if (!success)
        {
            return RedirectToAction("Error", "Home", new { code = 404 });
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetCurrentClientId();

        try
        {
            var model = await postService.GetDeleteModelAsync(id, userId);

            return View(model);
        }
        catch (NullReferenceException ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, DeletePostViewModel model)
    {
        var userId = GetCurrentClientId();
        var success = await postService.DeleteAsync(id, userId);

        if (!success)
        {
            return RedirectToAction("Error", "Home", new { code = 404 });
        }

        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Mine()
    {
        var userId = GetCurrentClientId();

        try
        {
            var model = await postService.GetMineAsync(userId);
            return View(model);
        }
        catch (NullReferenceException ex)
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
