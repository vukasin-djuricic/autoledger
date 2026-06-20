using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.States;

/// <summary>
/// A rejected entry is closed, but not a dead end: a clerk can reopen it back to Draft to
/// correct and resubmit it. Reopening clears the prior review so the workflow starts clean.
/// </summary>
public sealed class RejectedState : JournalEntryState
{
    public override JournalEntryStatus Status => JournalEntryStatus.Rejected;

    public override void Reopen(JournalEntry entry)
    {
        entry.ClearReview();
        entry.ApplyStatus(JournalEntryStatus.Draft);
    }
}
