namespace AutoLedger.Domain.Risk;

/// <summary>
/// Flags an entry whose amount deviates materially from the vendor's historical average.
/// The score is the deviation percentage (e.g. 35% over average → score 35), which means
/// the review threshold of 20 maps directly to the business rule "deviation &gt; 20% needs review".
/// Requires a minimum sample size; with too little history it defers to other strategies.
/// </summary>
public sealed class DeviationFromVendorAverageStrategy : IRiskStrategy
{
    public const int MinimumSamples = 3;

    public string Name => "Deviation from vendor average";

    public RiskResult Evaluate(RiskContext context)
    {
        var stats = context.VendorHistory;
        if (!context.HasVendor || stats.SampleCount < MinimumSamples || stats.Average <= 0)
            return RiskResult.Clear; // not enough signal — let NewPayeeStrategy handle cold start

        var deviation = Math.Abs(context.Amount - stats.Average);
        var deviationPct = (int)Math.Round(deviation / stats.Average * 100m);

        if (deviationPct <= 20)
            return RiskResult.Clear;

        var direction = context.Amount > stats.Average ? "above" : "below";
        return RiskResult.Flag(deviationPct,
            $"Amount {deviationPct}% {direction} this vendor's average of {stats.Average:0.00}");
    }
}
