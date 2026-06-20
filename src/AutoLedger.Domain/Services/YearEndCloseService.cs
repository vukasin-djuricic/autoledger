using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Reporting;

namespace AutoLedger.Domain.Services;

/// <summary>
/// Closes a fiscal year: builds a balanced closing entry that zeroes every revenue and expense
/// account into Retained Earnings (so P&amp;L accounts start the next year at zero and the net
/// result lands in equity), then posts it through the normal transactional path.
/// </summary>
public sealed class YearEndCloseService
{
    public const string RetainedEarningsCode = "3200";

    private readonly ILedgerQueries _queries;
    private readonly IAccountRepository _accounts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PostingService _posting;

    public YearEndCloseService(
        ILedgerQueries queries, IAccountRepository accounts, IUnitOfWork unitOfWork, PostingService posting)
    {
        _queries = queries;
        _accounts = accounts;
        _unitOfWork = unitOfWork;
        _posting = posting;
    }

    public async Task<JournalEntry> CloseYearAsync(int year, string performedBy, CancellationToken cancellationToken = default)
    {
        var reference = ReferenceFor(year);
        if (await _unitOfWork.JournalEntries.ReferenceExistsAsync(reference, cancellationToken))
            throw new InvalidOperationException($"Year {year} has already been closed.");

        var balances = await _queries.GetYearProfitAndLossByAccountAsync(year, cancellationToken);
        if (balances.Count == 0)
            throw new InvalidOperationException($"No posted profit-and-loss activity in {year} to close.");

        var retainedEarnings = (await _accounts.GetAllAsync(includeInactive: true, cancellationToken))
            .FirstOrDefault(a => a.Code == RetainedEarningsCode)
            ?? throw new InvalidOperationException($"Retained Earnings account ({RetainedEarningsCode}) is missing.");

        var entry = BuildClosingEntry(year, reference, performedBy, balances, retainedEarnings.Id);

        await _unitOfWork.JournalEntries.AddAsync(entry, cancellationToken);
        entry.SetRiskScore(0);
        entry.Submit();
        entry.Approve(performedBy);
        await _posting.PostAsync(entry, cancellationToken);

        return entry;
    }

    public static string ReferenceFor(int year) => $"YEC-{year}";

    /// <summary>
    /// Builds (without persisting) the closing entry: each revenue account is debited by its
    /// credit balance, each expense account credited by its debit balance, and the net result
    /// posted to Retained Earnings — leaving the entry balanced.
    /// </summary>
    public static JournalEntry BuildClosingEntry(
        int year, string reference, string performedBy, IReadOnlyList<AccountBalance> balances, int retainedEarningsAccountId)
    {
        var entry = new JournalEntry(new DateOnly(year, 12, 31), reference, $"Year-end close {year}", performedBy);

        decimal netIncome = 0m;
        foreach (var b in balances)
        {
            if (b.Amount <= 0) continue; // skip zero/contra balances
            if (b.Type == AccountType.Revenue)
            {
                entry.AddLine(b.AccountId, debit: b.Amount, credit: 0m); // clear the credit balance
                netIncome += b.Amount;
            }
            else // Expense
            {
                entry.AddLine(b.AccountId, debit: 0m, credit: b.Amount); // clear the debit balance
                netIncome -= b.Amount;
            }
        }

        if (netIncome > 0)
            entry.AddLine(retainedEarningsAccountId, debit: 0m, credit: netIncome);   // profit increases equity
        else if (netIncome < 0)
            entry.AddLine(retainedEarningsAccountId, debit: -netIncome, credit: 0m);  // loss decreases equity

        return entry;
    }
}
