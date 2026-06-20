using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Reporting;

namespace AutoLedger.Web.ViewModels;

public sealed class BalanceSheetViewModel
{
    public required IReadOnlyList<FinancialStatementLine> Rows { get; init; }

    /// <summary>Net income for the period not yet closed to equity — shown under equity so the sheet ties.</summary>
    public required decimal CurrentEarnings { get; init; }

    public IEnumerable<FinancialStatementLine> Assets => Rows.Where(r => r.Type == AccountType.Asset);
    public IEnumerable<FinancialStatementLine> Liabilities => Rows.Where(r => r.Type == AccountType.Liability);
    public IEnumerable<FinancialStatementLine> Equity => Rows.Where(r => r.Type == AccountType.Equity);

    public decimal TotalAssets => Assets.Sum(r => r.Amount);
    public decimal TotalLiabilities => Liabilities.Sum(r => r.Amount);
    public decimal TotalEquity => Equity.Sum(r => r.Amount) + CurrentEarnings;
    public decimal TotalLiabilitiesAndEquity => TotalLiabilities + TotalEquity;

    public bool IsBalanced => Math.Abs(TotalAssets - TotalLiabilitiesAndEquity) < 0.01m;
}
