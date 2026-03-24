using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;

using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.ViewModels.ErrorViewModels;
using ErasmusAtlas.Infrastructure.Repository.Interfaces;
using ErasmusAtlas.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace ErasmusAtlas.Controllers
{
    public class HomeController(
        IPostService postService,
        IProjectService projectService,
        IRepository<City, int> cityRepository,
        ILogger<HomeController> logger) 
        : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.LatestPosts = await postService.GetLatestAsync(6);
                ViewBag.LatestProjects = await projectService.GetLatestAsync(6);
                ViewBag.Cities = await cityRepository.GetAllAttached().Take(6).ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
