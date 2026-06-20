namespace AutoLedger.Domain.Reporting;

/// <summary>Headline counts for the dashboard KPI cards.</summary>
public sealed record DashboardSummary(int PendingCount, decimal PendingHeld, int PostedCount, int AutoPostedCount)
{
    /// <summary>Percentage of posted entries that were auto-approved (no human touch).</summary>
    public double AutoPostingRate => PostedCount == 0 ? 0 : Math.Round(AutoPostedCount * 100.0 / PostedCount, 1);
}
