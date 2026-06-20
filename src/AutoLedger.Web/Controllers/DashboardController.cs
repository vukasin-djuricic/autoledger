using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Enums;
using AutoLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLedger.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ILedgerQueries _queries;
    private readonly IJournalEntryRepository _entries;

    public DashboardController(ILedgerQueries queries, IJournalEntryRepository entries)
    {
        _queries = queries;
        _entries = entries;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var months = await _queries.GetCashFlowAsync(6, cancellationToken);
        var summary = await _queries.GetDashboardSummaryAsync(cancellationToken);
        var needsReview = await _entries.GetPagedAsync(JournalEntryStatus.PendingReview, 1, 5, cancellationToken);

        var model = new DashboardViewModel
        {
            Months = months,
            Summary = summary,
            NeedsReview = needsReview.Items
        };
        return View(model);
    }
}
