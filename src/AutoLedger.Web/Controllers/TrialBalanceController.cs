using AutoLedger.Domain.Abstractions;
using AutoLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLedger.Web.Controllers;

[Authorize]
public class TrialBalanceController : Controller
{
    private readonly ILedgerQueries _queries;

    public TrialBalanceController(ILedgerQueries queries) => _queries = queries;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var rows = await _queries.GetTrialBalanceAsync(cancellationToken);
        return View(new TrialBalanceViewModel { Rows = rows });
    }
}
