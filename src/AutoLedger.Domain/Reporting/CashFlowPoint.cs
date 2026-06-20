namespace AutoLedger.Domain.Reporting;

/// <summary>Monthly cash-flow aggregate: revenue in vs expenses out for one calendar month.</summary>
public sealed record CashFlowPoint(int Year, int Month, decimal Income, decimal Expense)
{
    public decimal Net => Income - Expense;
}
