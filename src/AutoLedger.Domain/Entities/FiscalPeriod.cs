using System.Globalization;
using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.Entities;

/// <summary>
/// One accounting period (a calendar month). Closing a period locks it so no further entries can
/// be posted into it — the standard period-close control of a general ledger.
/// </summary>
public class FiscalPeriod
{
    public int Id { get; private set; }
    public int Year { get; private set; }
    public int Month { get; private set; } // 1–12

    public FiscalPeriodStatus Status { get; private set; } = FiscalPeriodStatus.Open;

    private FiscalPeriod() { } // EF

    public FiscalPeriod(int year, int month)
    {
        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be 1–12.");
        Year = year;
        Month = month;
    }

    public bool IsClosed => Status == FiscalPeriodStatus.Closed;

    public string Label => new DateOnly(Year, Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture);

    public void Close()
    {
        if (IsClosed)
            throw new InvalidOperationException($"Period {Label} is already closed.");
        Status = FiscalPeriodStatus.Closed;
    }

    public void Reopen()
    {
        if (!IsClosed)
            throw new InvalidOperationException($"Period {Label} is already open.");
        Status = FiscalPeriodStatus.Open;
    }
}
