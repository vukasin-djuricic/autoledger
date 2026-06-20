namespace AutoLedger.Domain.Exceptions;

/// <summary>
/// Thrown when a journal entry is submitted/posted while its debits do not equal
/// its credits — the cardinal rule of double-entry bookkeeping.
/// </summary>
public class UnbalancedEntryException : Exception
{
    public decimal TotalDebit { get; }
    public decimal TotalCredit { get; }

    public UnbalancedEntryException(decimal totalDebit, decimal totalCredit)
        : base($"Entry is out of balance: debit {totalDebit:0.00} ≠ credit {totalCredit:0.00} (difference {totalDebit - totalCredit:0.00}).")
    {
        TotalDebit = totalDebit;
        TotalCredit = totalCredit;
    }
}
