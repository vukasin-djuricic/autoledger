namespace AutoLedger.Domain.Risk;

/// <summary>
/// Strategy pattern: one interchangeable rule that scores the risk of a journal entry.
/// New rules can be added without touching the assessment service — just register
/// another implementation.
/// </summary>
public interface IRiskStrategy
{
    /// <summary>Human-readable name of the rule (for diagnostics / reasons).</summary>
    string Name { get; }

    RiskResult Evaluate(RiskContext context);
}
