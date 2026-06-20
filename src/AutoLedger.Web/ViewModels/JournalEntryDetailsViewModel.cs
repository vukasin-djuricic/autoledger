using AutoLedger.Domain.Entities;

namespace AutoLedger.Web.ViewModels;

public sealed class JournalEntryDetailsViewModel
{
    public required JournalEntry Entry { get; init; }

    /// <summary>Audit-log records for this entry, oldest first — the activity timeline.</summary>
    public required IReadOnlyList<AuditLog> History { get; init; }
}
