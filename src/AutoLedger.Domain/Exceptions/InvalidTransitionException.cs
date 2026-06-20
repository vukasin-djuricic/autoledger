using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.Exceptions;

/// <summary>
/// Thrown when an illegal status transition is attempted (e.g. approving a Draft,
/// or modifying a Posted entry). Enforced by the State pattern.
/// </summary>
public class InvalidTransitionException : Exception
{
    public InvalidTransitionException(JournalEntryStatus from, string action)
        : base($"Cannot '{action}' a journal entry while it is in state '{from}'.")
    {
    }
}
