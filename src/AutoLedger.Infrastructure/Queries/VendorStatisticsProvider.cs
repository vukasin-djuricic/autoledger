using System.Data.Common;
using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Risk;
using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoLedger.Infrastructure.Queries;

/// <summary>
/// Computes a vendor's spend profile (count, mean, population std-dev) over its previously
/// posted entries — the statistical input to the deviation-based risk strategy. Aggregates at
/// the entry level first (a sub-query), then takes AVG and STDDEV_POP across entry amounts.
/// </summary>
public sealed class VendorStatisticsProvider : IVendorStatisticsProvider
{
    private readonly AppDbContext _db;

    public VendorStatisticsProvider(AppDbContext db) => _db = db;

    private const string Sql = """
        SELECT COUNT(*)                              AS sample_count,
               COALESCE(AVG(amount), 0)             AS avg_amount,
               COALESCE(STDDEV_POP(amount), 0)      AS std_amount
        FROM (
            SELECT e."Id", SUM(l."DebitAmount") AS amount
            FROM "JournalEntries"    e
            JOIN "JournalEntryLines" l ON l."JournalEntryId" = e."Id"
            WHERE e."VendorId" = @vendorId AND e."Status" = 'Posted'
            GROUP BY e."Id"
        ) entry_amounts;
        """;

    public async Task<VendorStatistics> GetVendorStatisticsAsync(int vendorId, CancellationToken cancellationToken = default)
    {
        var connection = _db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = Sql;

        var p = command.CreateParameter();
        p.ParameterName = "vendorId";
        p.Value = vendorId;
        command.Parameters.Add(p);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return VendorStatistics.None;

        var count = Convert.ToInt32(reader.GetValue(0));
        var avg = Convert.ToDecimal(reader.GetValue(1));
        var std = Convert.ToDecimal(reader.GetValue(2));
        return new VendorStatistics(count, avg, std);
    }
}
