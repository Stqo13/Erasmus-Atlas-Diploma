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

    [HttpGet]
    [Authorize(Roles = "Admin,VerifiedStudent")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateProjectViewModel
        {
            Cities = await projectService.GetCitiesAsync(),
            ProjectTypes = await projectService.GetProjectTypesAsync(),
            Tags = await projectService.GetTagsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,VerifiedStudent")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProjectViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Cities = await projectService.GetCitiesAsync();
            model.ProjectTypes = await projectService.GetProjectTypesAsync();
            model.Tags = await projectService.GetTagsAsync();
            return View(model);
        }

        try
        {
            string userId = GetCurrentClientId();
            await projectService.CreateAsync(model, userId);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating project.");

            model.Cities = await projectService.GetCitiesAsync();
            model.ProjectTypes = await projectService.GetProjectTypesAsync();
            model.Tags = await projectService.GetTagsAsync();

            ModelState.AddModelError(string.Empty, "Unable to create the project.");
            return View(model);
        }
    }

    private string GetCurrentClientId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
    }
}
