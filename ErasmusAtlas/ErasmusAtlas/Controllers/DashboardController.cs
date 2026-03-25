using ErasmusAtlas.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ErasmusAtlas.Controllers;

public class DashboardController(
    IDashboardService dashboardService,
    ILogger<DashboardController> logger)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await dashboardService.GetPageAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetOverview(string? country)
    {
        var model = await dashboardService.GetOverviewAsync(country);
        return Json(model);
    }
}
