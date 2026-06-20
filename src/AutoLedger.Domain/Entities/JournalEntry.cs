using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Exceptions;
using AutoLedger.Domain.States;

namespace AutoLedger.Domain.Entities;

/// <summary>
/// Aggregate root for a ledger transaction. Owns its posting <see cref="Lines"/> and
/// guards two invariants: debits must equal credits (double-entry), and status
/// transitions are driven exclusively through the State pattern.
/// </summary>
public class JournalEntry
{
    private const decimal Tolerance = 0.005m; // guards against decimal rounding noise

    private readonly List<JournalEntryLine> _lines = new();

    public int Id { get; private set; }
    public DateOnly Date { get; private set; }
    public string ReferenceNumber { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public JournalEntryStatus Status { get; private set; } = JournalEntryStatus.Draft;

    /// <summary>Risk score 0–100 assigned by the risk engine when the entry is submitted.</summary>
    public int RiskScore { get; private set; }

    public int? VendorId { get; private set; }
    public Vendor? Vendor { get; private set; }

    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public string? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    private JournalEntry() { } // EF

    public JournalEntry(DateOnly date, string referenceNumber, string description,
        string createdBy, int? vendorId = null)
    {
        Date = date;
        ReferenceNumber = referenceNumber;
        Description = description;
        CreatedBy = createdBy;
        VendorId = vendorId;
        CreatedAt = DateTime.UtcNow;
    }

    // ---- Lines ---------------------------------------------------------------

    public void AddLine(int accountId, decimal debit, decimal credit)
    {
        EnsureEditable();
        _lines.Add(new JournalEntryLine(accountId, debit, credit));
    }

    public decimal TotalDebit => _lines.Sum(l => l.DebitAmount);
    public decimal TotalCredit => _lines.Sum(l => l.CreditAmount);

    public bool IsBalanced =>
        _lines.Count >= 2 &&
        TotalDebit > 0 &&
        Math.Abs(TotalDebit - TotalCredit) < Tolerance;

    /// <summary>Throws <see cref="UnbalancedEntryException"/> unless debits equal credits.</summary>
    public void EnsureBalanced()
    {
        if (!IsBalanced)
            throw new UnbalancedEntryException(TotalDebit, TotalCredit);
    }

    // ---- State pattern facade ------------------------------------------------

    private JournalEntryState State => JournalEntryState.For(Status);

    public void Submit() => State.Submit(this);
    public void Approve(string reviewedBy) => State.Approve(this, reviewedBy);
    public void Reject(string reviewedBy, string reason) => State.Reject(this, reviewedBy, reason);
    public void Post() => State.Post(this);

    public void SetRiskScore(int score)
    {
        EnsureEditable();
        RiskScore = Math.Clamp(score, 0, 100);
    }

    // ---- Transition hooks (called only by state classes) ---------------------

    internal void ApplyStatus(JournalEntryStatus status) => Status = status;

    internal void ApplyReview(string reviewedBy, string? rejectionReason)
    {
        ReviewedBy = reviewedBy;
        ReviewedAt = DateTime.UtcNow;
        RejectionReason = rejectionReason;
    }

    private void EnsureEditable()
    {
        if (Status is JournalEntryStatus.Posted or JournalEntryStatus.Rejected)
            throw new InvalidTransitionException(Status, "edit");
    }
}
