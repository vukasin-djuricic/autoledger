using AutoLedger.Infrastructure.Identity;
using AutoLedger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLedger.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    public IActionResult Index() => View();

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetDemoData(CancellationToken cancellationToken)
    {
        await DbSeeder.ResetAsync(HttpContext.RequestServices, cancellationToken);
        TempData["Message"] = "Demo data has been reset to its seeded state.";
        return RedirectToAction(nameof(Index));
    }
}
