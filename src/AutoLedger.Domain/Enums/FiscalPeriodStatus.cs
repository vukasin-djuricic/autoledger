namespace AutoLedger.Domain.Enums;

/// <summary>Whether a fiscal period accepts new postings (Open) or is locked (Closed).</summary>
public enum FiscalPeriodStatus
{
    Open = 0,
    Closed = 1
}
