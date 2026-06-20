using AutoLedger.Domain.Reporting;

namespace AutoLedger.Web.ViewModels;

public sealed class TrialBalanceViewModel
{
    public required IReadOnlyList<TrialBalanceRow> Rows { get; init; }

    public decimal TotalDebit => Rows.Sum(r => r.Debit);
    public decimal TotalCredit => Rows.Sum(r => r.Credit);
    public bool IsBalanced => Math.Abs(TotalDebit - TotalCredit) < 0.01m;
}
