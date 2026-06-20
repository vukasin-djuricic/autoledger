namespace AutoLedger.Domain.Risk;

/// <summary>The contribution of a single strategy: a 0–100 score and an optional reason.</summary>
public readonly record struct RiskResult(int Score, string? Reason)
{
    public static readonly RiskResult Clear = new(0, null);

    public static RiskResult Flag(int score, string reason) =>
        new(Math.Clamp(score, 0, 100), reason);
}
