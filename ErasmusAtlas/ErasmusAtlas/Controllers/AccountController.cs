using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using ErasmusAtlas.Infrastructure.Models;
using ErasmusAtlas.ViewModels.AccountViewModels;

namespace ErasmusAtlas.Controllers;

public class AccountController(
    UserManager<ErasmusUser> userManager,
    SignInManager<ErasmusUser> signInManager)
    : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;  

        return View();   
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingByEmail = await userManager.FindByEmailAsync(model.Email);

        if (existingByEmail is not null)
        {
            ModelState.AddModelError(nameof(model.Email), "Email is already taken.");

            return View(model);
        }

        var existingByUsername = await userManager.FindByNameAsync(model.Username);

        if (existingByUsername is not null)
        {
            ModelState.AddModelError(nameof(model.Username), "Username is already taken.");

            return View(model);
        }

        var user = new ErasmusUser
        {
            UserName = model.Username,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }

            return View(model);
        }

        await signInManager.SignInAsync(user, isPersistent: false);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = model.UsernameOrEmail;

        if (model.UsernameOrEmail.Contains('@'))
        {
            var user = await userManager.FindByEmailAsync(model.UsernameOrEmail);

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

                return View(model);
            }

            username = user.UserName!;
        }

        var result = await signInManager.PasswordSignInAsync(
            username,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
