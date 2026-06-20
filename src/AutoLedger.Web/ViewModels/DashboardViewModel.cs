using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Reporting;

namespace AutoLedger.Web.ViewModels;

public sealed class DashboardViewModel
{
    public required IReadOnlyList<CashFlowPoint> Months { get; init; }
    public required DashboardSummary Summary { get; init; }
    public required IReadOnlyList<JournalEntry> NeedsReview { get; init; }

    public decimal MoneyIn => Months.Sum(m => m.Income);
    public decimal MoneyOut => Months.Sum(m => m.Expense);
    public decimal NetCashFlow => MoneyIn - MoneyOut;

    /// <summary>Largest single-month income or expense — used to scale the bar chart.</summary>
    public decimal ChartMax => Months.Count == 0
        ? 0m
        : Math.Max(Months.Max(m => m.Income), Months.Max(m => m.Expense));

    public bool HasComparison => Months.Count >= 2;

    /// <summary>Net change of the latest month versus the previous one, as a percentage.</summary>
    public double NetChangePercent
    {
        get
        {
            if (!HasComparison) return 0;
            var last = Months[^1].Net;
            var prev = Months[^2].Net;
            if (prev == 0) return 0;
            return (double)Math.Round((last - prev) / Math.Abs(prev) * 100, 1);
        }
    }

    public int BarHeight(decimal value) =>
        ChartMax == 0 ? 0 : (int)Math.Round(value / ChartMax * 100);
}
