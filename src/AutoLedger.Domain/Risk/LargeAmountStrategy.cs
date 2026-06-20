namespace AutoLedger.Domain.Risk;

/// <summary>
/// A hard ceiling: any entry above an absolute amount is reviewed regardless of vendor
/// history, so unusually large postings never slip through auto-approval.
/// </summary>
public sealed class LargeAmountStrategy : IRiskStrategy
{
    private readonly decimal _threshold;
    private const int FlagScore = 50;

    public LargeAmountStrategy(decimal threshold = 100_000m) => _threshold = threshold;

    public string Name => "Large absolute amount";

    public RiskResult Evaluate(RiskContext context) =>
        context.Amount > _threshold
            ? RiskResult.Flag(FlagScore, $"Amount exceeds the {_threshold:0,000} auto-approval ceiling")
            : RiskResult.Clear;
}
