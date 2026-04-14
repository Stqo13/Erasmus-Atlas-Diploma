using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.ViewModels.ProjectViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ErasmusAtlas.Controllers;

public class ProjectsController(
    IProjectService projectService,
    ILogger<ProjectsController> logger) 
    : Controller
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index([FromQuery] ProjectFilterViewModel filter)
    {
        try
        {
            var model = await projectService.GetAllFilteredAsync(filter);
            return View(model);
        }
        catch (NullReferenceException ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Details(Guid id)
    {
        string userId = GetCurrentClientId();

        try
        {
            var model = await projectService.GetByIdAsync(id, userId);

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
