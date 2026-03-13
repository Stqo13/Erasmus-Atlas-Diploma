using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.ViewModels.ProjectViewModels;

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
            var model = new ProjectsIndexPageViewModel
            {
                Filter = filter,
                Projects = await projectService.GetAllFilteredAsync(filter),
                Cities = await projectService.GetCitiesAsync(),
                ProjectTypes = await projectService.GetProjectTypesAsync(),
                Tags = await projectService.GetTagsAsync()
            };

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
        try
        {
            var model = await projectService.GetByIdAsync(id);

            return View(model);
        }
        catch (NullReferenceException ex)
        {
            logger.LogError($"{ex.Message}");
            return RedirectToAction("Error", "Home", new { code = 404 });
        }
    }
}
