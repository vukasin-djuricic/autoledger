namespace AutoLedger.Domain.Risk;

/// <summary>Combined outcome of all risk strategies for one entry.</summary>
/// <param name="Score">Aggregate 0–100 score (the highest individual strategy score).</param>
/// <param name="Reasons">Human-readable reasons from every triggered strategy.</param>
/// <param name="RequiresReview">True when the score reaches the review threshold.</param>
public sealed record RiskAssessment(int Score, IReadOnlyList<string> Reasons, bool RequiresReview);
