using AutoLedger.Domain.Risk;
using Xunit;

namespace AutoLedger.Tests.Domain;

public class RiskStrategyTests
{
    private static RiskContext WithVendor(decimal amount, int count, decimal avg, decimal std = 0m)
        => new(amount, HasVendor: true, new VendorStatistics(count, avg, std));

    // ---- Deviation from vendor average ----

    [Fact]
    public void Deviation_above_20_percent_is_flagged()
    {
        var result = new DeviationFromVendorAverageStrategy().Evaluate(WithVendor(1_300m, count: 5, avg: 1_000m));
        Assert.Equal(30, result.Score); // 30% over average
        Assert.NotNull(result.Reason);
    }

    [Fact]
    public void Deviation_within_tolerance_is_clear()
    {
        var result = new DeviationFromVendorAverageStrategy().Evaluate(WithVendor(1_100m, count: 5, avg: 1_000m));
        Assert.Equal(0, result.Score); // 10% — within the 20% tolerance
    }

    [Fact]
    public void Deviation_defers_on_cold_start()
    {
        var result = new DeviationFromVendorAverageStrategy().Evaluate(WithVendor(9_999m, count: 1, avg: 1_000m));
        Assert.Equal(0, result.Score); // not enough samples — NewPayee handles it
    }

    // ---- New payee (cold start) ----

    [Fact]
    public void New_payee_with_thin_history_is_flagged()
    {
        var result = new NewPayeeStrategy().Evaluate(WithVendor(500m, count: 1, avg: 500m));
        Assert.True(result.Score > 0);
    }

    [Fact]
    public void Established_vendor_is_not_a_new_payee()
    {
        var result = new NewPayeeStrategy().Evaluate(WithVendor(500m, count: 8, avg: 500m));
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void Entry_without_vendor_is_not_a_payee()
    {
        var result = new NewPayeeStrategy().Evaluate(new RiskContext(500m, HasVendor: false, VendorStatistics.None));
        Assert.Equal(0, result.Score);
    }

    // ---- Large absolute amount ----

    [Fact]
    public void Amount_over_ceiling_is_flagged()
    {
        var result = new LargeAmountStrategy(threshold: 100_000m)
            .Evaluate(new RiskContext(120_000m, false, VendorStatistics.None));
        Assert.True(result.Score > 0);
    }

    [Fact]
    public void Amount_under_ceiling_is_clear()
    {
        var result = new LargeAmountStrategy(threshold: 100_000m)
            .Evaluate(new RiskContext(80_000m, false, VendorStatistics.None));
        Assert.Equal(0, result.Score);
    }
}
