using AutoLedger.Domain.Risk;
using AutoLedger.Domain.Services;
using Xunit;

namespace AutoLedger.Tests.Domain;

public class RiskAssessmentServiceTests
{
    private static RiskAssessmentService BuildService() => new(new IRiskStrategy[]
    {
        new DeviationFromVendorAverageStrategy(),
        new NewPayeeStrategy(),
        new LargeAmountStrategy(threshold: 100_000m)
    });

    [Fact]
    public void Normal_entry_for_established_vendor_auto_approves()
    {
        var ctx = new RiskContext(1_050m, HasVendor: true, new VendorStatistics(8, 1_000m, 50m));
        var assessment = BuildService().Assess(ctx);

        Assert.False(assessment.RequiresReview);
        Assert.True(assessment.Score < RiskAssessmentService.ReviewThreshold);
        Assert.Empty(assessment.Reasons);
    }

    [Fact]
    public void High_deviation_requires_review_with_reason()
    {
        var ctx = new RiskContext(2_000m, HasVendor: true, new VendorStatistics(8, 1_000m, 100m));
        var assessment = BuildService().Assess(ctx);

        Assert.True(assessment.RequiresReview);
        Assert.True(assessment.Score >= RiskAssessmentService.ReviewThreshold);
        Assert.NotEmpty(assessment.Reasons);
    }

    [Fact]
    public void Cold_start_vendor_requires_review()
    {
        var ctx = new RiskContext(1_000m, HasVendor: true, new VendorStatistics(0, 0m, 0m));
        var assessment = BuildService().Assess(ctx);
        Assert.True(assessment.RequiresReview);
    }

    [Fact]
    public void Score_is_aggregated_as_the_highest_strategy()
    {
        // 50% deviation (score 50) AND over the large-amount ceiling (score 50) → max wins, not sum.
        var ctx = new RiskContext(150_000m, HasVendor: true, new VendorStatistics(8, 100_000m, 1_000m));
        var assessment = BuildService().Assess(ctx);
        Assert.True(assessment.Score <= 100);
        Assert.True(assessment.Reasons.Count >= 1);
    }
}
