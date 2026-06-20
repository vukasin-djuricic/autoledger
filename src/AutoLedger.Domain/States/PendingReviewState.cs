using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.States;

/// <summary>Awaiting a human (or automated) decision. Can be approved or rejected.</summary>
public sealed class PendingReviewState : JournalEntryState
{
    public override JournalEntryStatus Status => JournalEntryStatus.PendingReview;

    public override void Approve(JournalEntry entry, string reviewedBy)
    {
        entry.ApplyReview(reviewedBy, rejectionReason: null);
        entry.ApplyStatus(JournalEntryStatus.Approved);
    }

    public override void Reject(JournalEntry entry, string reviewedBy, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A rejection reason is required.", nameof(reason));

        entry.ApplyReview(reviewedBy, rejectionReason: reason);
        entry.ApplyStatus(JournalEntryStatus.Rejected);
    }
}
