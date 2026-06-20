using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.States;

/// <summary>Terminal state. A rejected entry is closed and cannot transition further.</summary>
public sealed class RejectedState : JournalEntryState
{
    public override JournalEntryStatus Status => JournalEntryStatus.Rejected;
}
