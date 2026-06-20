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

    /// <summary>Income vs expense aggregated by month for the last <paramref name="months"/> months.</summary>
    Task<IReadOnlyList<CashFlowPoint>> GetCashFlowAsync(int months, CancellationToken cancellationToken = default);

    /// <summary>Headline counts for the dashboard KPI cards.</summary>
    Task<DashboardSummary> GetDashboardSummaryAsync(CancellationToken cancellationToken = default);
}
