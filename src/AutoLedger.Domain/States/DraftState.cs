using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.States;

/// <summary>A new, editable entry. The only legal transition is to submit it for review.</summary>
public sealed class DraftState : JournalEntryState
{
    public override JournalEntryStatus Status => JournalEntryStatus.Draft;

    public override void Submit(JournalEntry entry)
    {
        entry.EnsureBalanced();
        entry.ApplyStatus(JournalEntryStatus.PendingReview);
    }
}
