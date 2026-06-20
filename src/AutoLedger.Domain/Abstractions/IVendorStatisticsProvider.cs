using AutoLedger.Domain.Risk;

namespace AutoLedger.Domain.Abstractions;

/// <summary>
/// Supplies the vendor spend statistics (AVG + STDDEV over prior posted entries) that the
/// risk strategies consume. Backed by a hand-written aggregate SQL query.
/// </summary>
public interface IVendorStatisticsProvider
{
    Task<VendorStatistics> GetVendorStatisticsAsync(int vendorId, CancellationToken cancellationToken = default);
}
