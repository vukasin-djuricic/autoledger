namespace AutoLedger.Domain.Risk;

/// <summary>
/// Handles the cold-start problem: a vendor with little or no payment history can't be
/// judged statistically, so any entry against one is routed to a human for review.
/// </summary>
public sealed class NewPayeeStrategy : IRiskStrategy
{
    private const int ColdStartScore = 60;

    public string Name => "New / unestablished payee";

    public RiskResult Evaluate(RiskContext context)
    {
        if (!context.HasVendor)
            return RiskResult.Clear; // entries without a vendor (e.g. internal accruals) aren't payees

        if (context.VendorHistory.SampleCount >= DeviationFromVendorAverageStrategy.MinimumSamples)
            return RiskResult.Clear;

        return RiskResult.Flag(ColdStartScore,
            "Insufficient payment history for this vendor (cold start)");
    }
}
