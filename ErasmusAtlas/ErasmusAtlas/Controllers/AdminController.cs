using ErasmusAtlas.Core.Interfaces;
using ErasmusAtlas.ViewModels.AdminViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ErasmusAtlas.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    IAdminService adminService)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = await adminService.GetDashboardAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var model = await adminService.GetUsersAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var model = await adminService.GetEditUserAsync(id);

        if (model == null)
        {
            return RedirectToAction("Error", "Home", new { code = 404 });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(AdminEditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var reload = await adminService.GetEditUserAsync(model.Id);
            if (reload == null)
            {
                return RedirectToAction("Error", "Home", new { code = 404 });
            }

            reload.Email = model.Email;
            reload.UserName = model.UserName;
            reload.FirstName = model.FirstName;
            reload.LastName = model.LastName;
            reload.DisplayName = model.DisplayName;
            reload.Bio = model.Bio;
            reload.SelectedRoles = model.SelectedRoles;

            return View(reload);
        }

        var success = await adminService.EditUserAsync(model);

        if (!success)
        {
            return RedirectToAction("Error", "Home", new { code = 500 });
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var success = await adminService.DeleteUserAsync(id);

        if (!success)
        {
            return RedirectToAction("Error", "Home", new { code = 404 });
        }

        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> Posts()
    {
        var model = await adminService.GetPostsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        var success = await adminService.DeletePostAsync(id);

        if (!success)
        {
            return RedirectToAction("Error", "Home", new { code = 404 });
        }

        return RedirectToAction(nameof(Posts));
    }

    [HttpGet]
    public async Task<IActionResult> Projects()
    {
        var model = await adminService.GetProjectsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var success = await adminService.DeleteProjectAsync(id);

        if (!success)
        {
            return RedirectToAction("Error", "Home", new { code = 404 });
        }

        return RedirectToAction(nameof(Projects));
    }
}
