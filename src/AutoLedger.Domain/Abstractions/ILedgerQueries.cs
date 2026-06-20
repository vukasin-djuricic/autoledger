using AutoLedger.Domain.Reporting;

namespace AutoLedger.Domain.Abstractions;

/// <summary>
/// Read-side analytical queries that don't map cleanly onto the aggregate — implemented
/// with hand-written SQL in the infrastructure layer (Trial Balance, Cash Flow).
/// </summary>
public interface ILedgerQueries
{
    /// <summary>Per-account net debit/credit totals over all posted entries.</summary>
    Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Every posted line for one account, oldest first, with a running balance signed to the
    /// account's normal side. Drill-down from a Trial Balance row.
    /// </summary>
    Task<IReadOnlyList<AccountLedgerRow>> GetAccountLedgerAsync(
        int accountId, bool normalBalanceIsDebit, CancellationToken cancellationToken = default);

    /// <summary>Income vs expense aggregated by month for the last <paramref name="months"/> months.</summary>
    Task<IReadOnlyList<CashFlowPoint>> GetCashFlowAsync(int months, CancellationToken cancellationToken = default);

    /// <summary>Revenue and expense accounts with their net amounts (the Income Statement / P&amp;L).</summary>
    Task<IReadOnlyList<FinancialStatementLine>> GetIncomeStatementAsync(CancellationToken cancellationToken = default);

    /// <summary>Asset, liability and equity accounts with their net amounts (the Balance Sheet).</summary>
    Task<IReadOnlyList<FinancialStatementLine>> GetBalanceSheetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Net balance per revenue/expense account (positive on its normal side) for a fiscal year —
    /// the input to the year-end closing entry.
    /// </summary>
    Task<IReadOnlyList<AccountBalance>> GetYearProfitAndLossByAccountAsync(int year, CancellationToken cancellationToken = default);

    /// <summary>Headline counts for the dashboard KPI cards.</summary>
    Task<DashboardSummary> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
}
