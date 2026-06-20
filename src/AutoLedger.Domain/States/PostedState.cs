using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.States;

/// <summary>
/// Terminal state. A posted entry is immutable — it lives in the permanent ledger and
/// can never be edited or deleted (audit integrity). Every transition is illegal.
/// </summary>
public sealed class PostedState : JournalEntryState
{
    public override JournalEntryStatus Status => JournalEntryStatus.Posted;
}
