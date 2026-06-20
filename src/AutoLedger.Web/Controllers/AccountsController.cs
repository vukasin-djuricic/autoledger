using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Infrastructure.Identity;
using AutoLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLedger.Web.Controllers;

[Authorize]
public class AccountsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.Accounts.GetAllAsync(includeInactive: true, cancellationToken);
        return View(accounts);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Controller)]
    public IActionResult Create() => View(new AccountFormViewModel());

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccountFormViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            if (await _unitOfWork.Accounts.CodeExistsAsync(model.Code, cancellationToken))
            {
                ModelState.AddModelError(nameof(model.Code), "An account with this code already exists.");
            }
            else
            {
                var account = new Account(model.Code.Trim(), model.Name.Trim(), model.Type);
                await _unitOfWork.Accounts.AddAsync(account, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                TempData["Message"] = $"Account {account.Code} created.";
                return RedirectToAction(nameof(Index));
            }
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = Roles.Controller)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id, cancellationToken);
        if (account is null) return NotFound();

        return View(new AccountFormViewModel
        {
            Id = account.Id,
            Code = account.Code,
            Name = account.Name,
            Type = account.Type,
        });
    }

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AccountFormViewModel model, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id, cancellationToken);
        if (account is null) return NotFound();

        // Code and type are fixed once created — only the name is editable.
        if (!string.IsNullOrWhiteSpace(model.Name))
        {
            try
            {
                account.Rename(model.Name);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                TempData["Message"] = $"Account {account.Code} updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(model.Name), ex.Message);
            }
        }
        else
        {
            ModelState.AddModelError(nameof(model.Name), "Account name is required.");
        }

        model.Id = account.Id;
        model.Code = account.Code;
        model.Type = account.Type;
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Controller)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Accounts.GetByIdAsync(id, cancellationToken);
        if (account is null) return NotFound();

        if (account.IsActive) account.Deactivate(); else account.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        TempData["Message"] = $"Account {account.Code} {(account.IsActive ? "activated" : "deactivated")}.";
        return RedirectToAction(nameof(Index));
    }
}
