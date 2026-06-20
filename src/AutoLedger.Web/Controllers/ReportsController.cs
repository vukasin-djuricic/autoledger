using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Enums;
using AutoLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoLedger.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly ILedgerQueries _queries;

    public ReportsController(ILedgerQueries queries) => _queries = queries;

    public async Task<IActionResult> IncomeStatement(CancellationToken cancellationToken)
    {
        var rows = await _queries.GetIncomeStatementAsync(cancellationToken);
        return View(new IncomeStatementViewModel { Rows = rows });
    }

    public async Task<IActionResult> BalanceSheet(CancellationToken cancellationToken)
    {
        var rows = await _queries.GetBalanceSheetAsync(cancellationToken);

        // Current-period net income isn't closed to equity yet (see year-end close); fold it into
        // equity so the accounting equation (Assets = Liabilities + Equity) ties out.
        var pl = await _queries.GetIncomeStatementAsync(cancellationToken);
        var netIncome = pl.Where(r => r.Type == AccountType.Revenue).Sum(r => r.Amount)
                        - pl.Where(r => r.Type == AccountType.Expense).Sum(r => r.Amount);

        return View(new BalanceSheetViewModel { Rows = rows, CurrentEarnings = netIncome });
    }
}
