using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Reporting;

namespace AutoLedger.Web.ViewModels;

public sealed class IncomeStatementViewModel
{
    public required IReadOnlyList<FinancialStatementLine> Rows { get; init; }

    public IEnumerable<FinancialStatementLine> Revenue => Rows.Where(r => r.Type == AccountType.Revenue);
    public IEnumerable<FinancialStatementLine> Expenses => Rows.Where(r => r.Type == AccountType.Expense);

    public decimal TotalRevenue => Revenue.Sum(r => r.Amount);
    public decimal TotalExpenses => Expenses.Sum(r => r.Amount);
    public decimal NetIncome => TotalRevenue - TotalExpenses;
}
