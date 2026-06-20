using AutoLedger.Domain.Risk;

namespace AutoLedger.Domain.Services;

/// <summary>
/// Runs every registered <see cref="IRiskStrategy"/> over an entry and combines the results
/// into a single <see cref="RiskAssessment"/>. New rules are added purely by registering
/// another strategy in DI — this service never changes (Open/Closed principle).
/// </summary>
public sealed class RiskAssessmentService
{
    /// <summary>
    /// Scores at or above this threshold are routed to human review. Set to 20 so it lines up
    /// with the business rule "amount deviation &gt; 20% from the vendor average needs review".
    /// </summary>
    public const int ReviewThreshold = 20;

    private readonly IReadOnlyList<IRiskStrategy> _strategies;

    public RiskAssessmentService(IEnumerable<IRiskStrategy> strategies)
        => _strategies = strategies.ToList();

    public RiskAssessment Assess(RiskContext context)
    {
        var results = _strategies.Select(s => s.Evaluate(context)).ToList();

        var score = results.Count == 0 ? 0 : results.Max(r => r.Score);
        var reasons = results
            .Where(r => r.Score > 0 && !string.IsNullOrWhiteSpace(r.Reason))
            .Select(r => r.Reason!)
            .ToList();

        return new RiskAssessment(score, reasons, RequiresReview: score >= ReviewThreshold);
    }
}
