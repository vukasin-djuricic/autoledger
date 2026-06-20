using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Enums;
using AutoLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLedger.Web.Controllers;

[Authorize]
public class TransactionsController : Controller
{
    private const int PageSize = 8;
    private readonly IJournalEntryRepository _entries;

    public TransactionsController(IJournalEntryRepository entries) => _entries = entries;

    public async Task<IActionResult> Index(string? status, int page = 1, CancellationToken cancellationToken = default)
    {
        JournalEntryStatus? parsed =
            Enum.TryParse<JournalEntryStatus>(status, out var s) ? s : null;

        var result = await _entries.GetPagedAsync(parsed, page, PageSize, cancellationToken);

        return View(new TransactionsIndexViewModel { Page = result, Status = parsed });
    }
}
