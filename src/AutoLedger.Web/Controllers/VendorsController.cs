using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Infrastructure.Identity;
using AutoLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLedger.Web.Controllers;

[Authorize]
public class VendorsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public VendorsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var vendors = await _unitOfWork.Vendors.GetAllAsync(includeInactive: true, cancellationToken);
        return View(vendors);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Controller)]
    public IActionResult Create() => View(new VendorFormViewModel());

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VendorFormViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var vendor = new Vendor(model.Name.Trim());
            await _unitOfWork.Vendors.AddAsync(vendor, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            TempData["Message"] = $"Vendor “{vendor.Name}” created.";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Controller)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(id, cancellationToken);
        if (vendor is null) return NotFound();

        return View(new VendorFormViewModel { Id = vendor.Id, Name = vendor.Name });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VendorFormViewModel model, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(id, cancellationToken);
        if (vendor is null) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                vendor.Rename(model.Name);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                TempData["Message"] = $"Vendor “{vendor.Name}” updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(model.Name), ex.Message);
            }
        }

        model.Id = vendor.Id;
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(id, cancellationToken);
        if (vendor is null) return NotFound();

        if (vendor.IsActive) vendor.Deactivate(); else vendor.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        TempData["Message"] = $"Vendor “{vendor.Name}” {(vendor.IsActive ? "activated" : "deactivated")}.";
        return RedirectToAction(nameof(Index));
    }
}
