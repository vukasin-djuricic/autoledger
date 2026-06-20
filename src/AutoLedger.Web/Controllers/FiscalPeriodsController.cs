using AutoLedger.Domain.Abstractions;
using AutoLedger.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLedger.Web.Controllers;

[Authorize]
public class FiscalPeriodsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public FiscalPeriodsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var periods = await _unitOfWork.FiscalPeriods.GetAllAsync(cancellationToken);
        return View(periods);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id, CancellationToken cancellationToken)
        => await ToggleAsync(id, close: true, cancellationToken);

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reopen(int id, CancellationToken cancellationToken)
        => await ToggleAsync(id, close: false, cancellationToken);

    private async Task<IActionResult> ToggleAsync(int id, bool close, CancellationToken cancellationToken)
    {
        var period = await _unitOfWork.FiscalPeriods.GetByIdAsync(id, cancellationToken);
        if (period is null) return NotFound();

        try
        {
            if (close) period.Close(); else period.Reopen();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            TempData["Message"] = $"Period {period.Label} {(close ? "closed" : "reopened")}.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
