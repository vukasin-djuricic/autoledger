using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Risk;

namespace AutoLedger.Domain.Services;

/// <summary>
/// Orchestrates the approval workflow: scores risk on submission, then either auto-posts
/// low-risk entries or routes risky ones to a human. Also handles the manual approve/reject
/// decisions. Combines the State pattern (entry transitions), the Strategy pattern (risk),
/// and the Unit of Work (atomic posting).
/// </summary>
public sealed class WorkflowEngine
{
    private readonly RiskAssessmentService _risk;
    private readonly PostingService _posting;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVendorStatisticsProvider _vendorStats;

    public WorkflowEngine(
        RiskAssessmentService risk,
        PostingService posting,
        IUnitOfWork unitOfWork,
        IVendorStatisticsProvider vendorStats)
    {
        _risk = risk;
        _posting = posting;
        _unitOfWork = unitOfWork;
        _vendorStats = vendorStats;
    }

    /// <summary>
    /// Submits a draft: assesses risk, then auto-approves &amp; posts if it's safe, or leaves it
    /// in PendingReview for a human. Returns the assessment for display.
    /// </summary>
    public async Task<RiskAssessment> SubmitAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        entry.EnsureBalanced();

        var stats = entry.VendorId is int vendorId
            ? await _vendorStats.GetVendorStatisticsAsync(vendorId, cancellationToken)
            : VendorStatistics.None;

        var context = new RiskContext(entry.TotalDebit, entry.VendorId is not null, stats);
        var assessment = _risk.Assess(context);

        entry.SetRiskScore(assessment.Score);
        entry.Submit(); // Draft -> PendingReview

        if (assessment.RequiresReview)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            entry.Approve("system (auto)");        // PendingReview -> Approved
            await _posting.PostAsync(entry, cancellationToken); // -> Posted (transactional)
        }

        return assessment;
    }

    /// <summary>Manual approval by a controller — approves then immediately posts.</summary>
    public async Task ApproveAsync(JournalEntry entry, string reviewedBy, CancellationToken cancellationToken = default)
    {
        entry.Approve(reviewedBy);
        await _posting.PostAsync(entry, cancellationToken);
    }

    /// <summary>Manual rejection by a controller — requires a reason.</summary>
    public async Task RejectAsync(JournalEntry entry, string reviewedBy, string reason, CancellationToken cancellationToken = default)
    {
        entry.Reject(reviewedBy, reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
