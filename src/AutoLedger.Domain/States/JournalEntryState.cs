using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Exceptions;

namespace AutoLedger.Domain.States;

/// <summary>
/// State pattern: each lifecycle status is a concrete state class that knows which
/// transitions are legal from it. Illegal transitions throw
/// <see cref="InvalidTransitionException"/>. The default for every action is "not
/// allowed" — concrete states override only the transitions they permit.
///
/// States are stateless singletons; the <see cref="JournalEntry"/> being acted on
/// is passed in, keeping the pattern free of duplicated mutable state.
/// </summary>
public abstract class JournalEntryState
{
    public abstract JournalEntryStatus Status { get; }

    public virtual void Submit(JournalEntry entry) => throw Illegal(entry, "submit");
    public virtual void Approve(JournalEntry entry, string reviewedBy) => throw Illegal(entry, "approve");
    public virtual void Reject(JournalEntry entry, string reviewedBy, string reason) => throw Illegal(entry, "reject");
    public virtual void Post(JournalEntry entry) => throw Illegal(entry, "post");

    protected static InvalidTransitionException Illegal(JournalEntry entry, string action) =>
        new(entry.Status, action);

    /// <summary>Resolves the state object for a given status.</summary>
    public static JournalEntryState For(JournalEntryStatus status) => status switch
    {
        JournalEntryStatus.Draft => Draft,
        JournalEntryStatus.PendingReview => PendingReview,
        JournalEntryStatus.Approved => Approved,
        JournalEntryStatus.Posted => Posted,
        JournalEntryStatus.Rejected => Rejected,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown status.")
    };

    public static readonly JournalEntryState Draft = new DraftState();
    public static readonly JournalEntryState PendingReview = new PendingReviewState();
    public static readonly JournalEntryState Approved = new ApprovedState();
    public static readonly JournalEntryState Posted = new PostedState();
    public static readonly JournalEntryState Rejected = new RejectedState();
}
