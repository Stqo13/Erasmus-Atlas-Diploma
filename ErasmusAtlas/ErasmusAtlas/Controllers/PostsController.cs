using ErasmusAtlas.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErasmusAtlas.Controllers;

public class PostsController(
    IPostService postService)
    : Controller
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index(string? city, string? topic)
    {
        try
        {
            var models = await postService
                .GetAllFilteredAsync(city, topic);

            return View(models);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("Unable to find posts!", ex.Message);
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }
}
