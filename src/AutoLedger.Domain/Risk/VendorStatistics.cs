namespace AutoLedger.Domain.Risk;

/// <summary>
/// Historical spend profile for a vendor, computed by the data layer
/// (AVG + STDDEV over previously posted entries). Used by the risk strategies.
/// </summary>
/// <param name="SampleCount">Number of prior posted entries for the vendor.</param>
/// <param name="Average">Mean amount of those entries.</param>
/// <param name="StandardDeviation">Population standard deviation of those amounts.</param>
public readonly record struct VendorStatistics(int SampleCount, decimal Average, decimal StandardDeviation)
{
    public static readonly VendorStatistics None = new(0, 0m, 0m);
}
