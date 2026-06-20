using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Exceptions;
using AutoLedger.Domain.Services;
using AutoLedger.Infrastructure.Identity;
using AutoLedger.Infrastructure.Persistence;
using AutoLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AutoLedger.Web.Controllers;

[Authorize]
public class JournalEntriesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly WorkflowEngine _workflow;
    private readonly AppDbContext _db; // read-only lookups for the create form dropdowns

    public JournalEntriesController(IUnitOfWork unitOfWork, WorkflowEngine workflow, AppDbContext db)
    {
        _unitOfWork = unitOfWork;
        _workflow = workflow;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreateJournalEntryViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateJournalEntryViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var entry = new JournalEntry(model.Date, model.Reference, model.Description,
                    User.Identity?.Name ?? "unknown", model.VendorId);

                foreach (var line in model.Lines)
                {
                    if (line.AccountId is not int accountId) continue;
                    var debit = line.Debit ?? 0m;
                    var credit = line.Credit ?? 0m;
                    if (debit == 0 && credit == 0) continue;
                    entry.AddLine(accountId, debit, credit);
                }

                await _unitOfWork.JournalEntries.AddAsync(entry, cancellationToken);
                var assessment = await _workflow.SubmitAsync(entry, cancellationToken);

                TempData["Message"] = assessment.RequiresReview
                    ? $"Entry {entry.ReferenceNumber} submitted for review (risk score {assessment.Score})."
                    : $"Entry {entry.ReferenceNumber} auto-approved and posted.";

                return RedirectToAction(nameof(Details), new { id = entry.Id });
            }
            catch (UnbalancedEntryException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await PopulateOptionsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(id, cancellationToken);
        return entry is null ? NotFound() : View(entry);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(id, cancellationToken);
        if (entry is null) return NotFound();

        try
        {
            await _workflow.ApproveAsync(entry, User.Identity?.Name ?? "controller", cancellationToken);
            TempData["Message"] = $"Entry {entry.ReferenceNumber} approved and posted.";
        }
        catch (Exception ex) when (ex is InvalidTransitionException or UnbalancedEntryException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(id, cancellationToken);
        if (entry is null) return NotFound();

        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["Error"] = "A rejection reason is required.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            await _workflow.RejectAsync(entry, User.Identity?.Name ?? "controller", reason, cancellationToken);
            TempData["Message"] = $"Entry {entry.ReferenceNumber} rejected.";
        }
        catch (InvalidTransitionException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reverse(int id, CancellationToken cancellationToken)
    {
        var entry = await _unitOfWork.JournalEntries.GetByIdAsync(id, cancellationToken);
        if (entry is null) return NotFound();

        try
        {
            var reversal = await _workflow.ReverseAsync(entry, User.Identity?.Name ?? "controller", cancellationToken);
            TempData["Message"] = $"Entry {entry.ReferenceNumber} reversed by {reversal.ReferenceNumber}.";
            return RedirectToAction(nameof(Details), new { id = reversal.Id });
        }
        catch (Exception ex) when (ex is InvalidTransitionException or InvalidOperationException or UnbalancedEntryException)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task PopulateOptionsAsync(CreateJournalEntryViewModel model, CancellationToken cancellationToken)
    {
        model.AccountOptions = await _db.Accounts
            .OrderBy(a => a.Code)
            .Select(a => new SelectListItem($"{a.Code} · {a.Name}", a.Id.ToString()))
            .ToListAsync(cancellationToken);

        model.VendorOptions = await _db.Vendors
            .OrderBy(v => v.Name)
            .Select(v => new SelectListItem(v.Name, v.Id.ToString()))
            .ToListAsync(cancellationToken);
    }
}
