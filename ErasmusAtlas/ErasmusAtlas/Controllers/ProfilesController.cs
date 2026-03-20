using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.ViewModels.AccountViewModels;

namespace ErasmusAtlas.Controllers;

[Authorize]
public class ProfilesController(
    IProfileService profileService)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Me()
    {
        string userId = GetCurrentClientId();
        var model = await profileService.GetProfileAsync(userId, userId);

        if (model == null)
        {
            return RedirectToAction("Error", "Home", new { code = 404 });
        }

        return View("Details", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        string? currentUserId = GetCurrentClientId();
        var model = await profileService.GetProfileAsync(id, currentUserId);

        if (model == null)
        {
            return RedirectToAction("Error", "Home", new { code = 404 });
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        string userId = GetCurrentClientId();
        var model = await profileService.GetEditModelAsync(userId);

        if (model == null)
        {
            return RedirectToAction("Error", "Home", new { code = 404 });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string userId = GetCurrentClientId();
        bool success = await profileService.EditAsync(userId, model);

        if (!success)
        {
            return RedirectToAction("Error", "Home", new { code = 404 });
        }

        return RedirectToAction(nameof(Me));
    }

    private string GetCurrentClientId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
