using AutoLedger.Domain.Abstractions;
using AutoLedger.Infrastructure.Persistence;
using AutoLedger.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoLedger.Web.Controllers;

[Authorize]
public class TrialBalanceController : Controller
{
    private readonly ILedgerQueries _queries;
    private readonly AppDbContext _db; // read-only account lookup for the drill-down header

    public TrialBalanceController(ILedgerQueries queries, AppDbContext db)
    {
        _queries = queries;
        _db = db;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var rows = await _queries.GetTrialBalanceAsync(cancellationToken);
        return View(new TrialBalanceViewModel { Rows = rows });
    }

    public async Task<IActionResult> Ledger(int accountId, CancellationToken cancellationToken)
    {
        var account = await _db.Accounts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == accountId, cancellationToken);
        if (account is null) return NotFound();

        var rows = await _queries.GetAccountLedgerAsync(accountId, account.NormalBalanceIsDebit, cancellationToken);

        return View(new AccountLedgerViewModel
        {
            AccountId = account.Id,
            Code = account.Code,
            Name = account.Name,
            Type = account.Type,
            Rows = rows,
        });
    }
}
