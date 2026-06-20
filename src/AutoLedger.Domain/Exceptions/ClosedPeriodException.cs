namespace AutoLedger.Domain.Exceptions;

/// <summary>Thrown when an entry would be posted into a fiscal period that has been closed.</summary>
public class ClosedPeriodException : Exception
{
    public ClosedPeriodException(DateOnly date)
        : base($"The fiscal period for {date:yyyy-MM} is closed; no entries can be posted into it.")
    {
    }
}
