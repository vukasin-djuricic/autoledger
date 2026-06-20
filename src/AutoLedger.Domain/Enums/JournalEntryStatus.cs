namespace AutoLedger.Domain.Enums;

/// <summary>
/// Lifecycle of a journal entry. Transitions between these values are governed
/// by the State pattern (see <c>Domain.States</c>) — not arbitrary assignment.
/// </summary>
public enum JournalEntryStatus
{
    Draft = 0,
    PendingReview = 1,
    Approved = 2,
    Posted = 3,
    Rejected = 4
}
