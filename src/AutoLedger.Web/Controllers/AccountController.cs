using AutoLedger.Infrastructure.Identity;
using AutoLedger.Infrastructure.Persistence;
using AutoLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AutoLedger.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(SignInManager<ApplicationUser> signInManager)
        => _signInManager = signInManager;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
            return RedirectToAction("Index", "Dashboard");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    /// <summary>Convenience for the demo: one-click sign-in as a seeded account.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoLogin(string role)
    {
        var email = role == "Clerk" ? DbSeeder.ClerkEmail : DbSeeder.ControllerEmail;
        await _signInManager.PasswordSignInAsync(email, DbSeeder.DemoPassword, isPersistent: false, lockoutOnFailure: false);
        return RedirectToAction("Index", "Dashboard");
    }
}
