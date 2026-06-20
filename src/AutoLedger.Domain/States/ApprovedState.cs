using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.States;

/// <summary>Approved but not yet written to the ledger. The only step left is to post it.</summary>
public sealed class ApprovedState : JournalEntryState
{
    public override JournalEntryStatus Status => JournalEntryStatus.Approved;

    public override void Post(JournalEntry entry)
    {
        entry.EnsureBalanced();
        entry.ApplyStatus(JournalEntryStatus.Posted);
    }
}
