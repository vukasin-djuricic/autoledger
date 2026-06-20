using System.Data.Common;
using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Reporting;
using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

// (dashboard summary uses LINQ; the two reports use raw SQL)

namespace AutoLedger.Infrastructure.Queries;

/// <summary>
/// Read-side analytical queries written as hand-crafted SQL (the same statements are mirrored
/// in <c>Queries/*.sql</c> for reference). These are the "complex queries" the role calls for:
/// multi-table joins, GROUP BY, conditional aggregation (CASE WHEN), and date bucketing.
/// </summary>
public sealed class LedgerQueries : ILedgerQueries
{
    private readonly AppDbContext _db;

    public LedgerQueries(AppDbContext db) => _db = db;

    // Trial Balance: each account's net movement placed on the correct side via CASE WHEN.
    // Across all accounts, total debit must equal total credit — proving the books balance.
    private const string TrialBalanceSql = """
        SELECT a."Code"  AS account_code,
               a."Name"  AS account_name,
               a."Type"  AS account_type,
               CASE WHEN SUM(l."DebitAmount" - l."CreditAmount") >= 0
                    THEN  SUM(l."DebitAmount" - l."CreditAmount") ELSE 0 END AS debit,
               CASE WHEN SUM(l."DebitAmount" - l."CreditAmount") < 0
                    THEN -SUM(l."DebitAmount" - l."CreditAmount") ELSE 0 END AS credit
        FROM "JournalEntryLines" l
        JOIN "JournalEntries" e ON e."Id" = l."JournalEntryId"
        JOIN "Accounts"       a ON a."Id" = l."AccountId"
        WHERE e."Status" = 'Posted'
        GROUP BY a."Id", a."Code", a."Name", a."Type"
        ORDER BY a."Code";
        """;

    // Cash Flow: revenue in vs expenses out, bucketed by calendar month over a recent window.
    private const string CashFlowSql = """
        SELECT EXTRACT(YEAR  FROM e."Date")::int AS yr,
               EXTRACT(MONTH FROM e."Date")::int AS mo,
               COALESCE(SUM(CASE WHEN a."Type" = 'Revenue'
                                 THEN l."CreditAmount" - l."DebitAmount" ELSE 0 END), 0) AS income,
               COALESCE(SUM(CASE WHEN a."Type" = 'Expense'
                                 THEN l."DebitAmount" - l."CreditAmount" ELSE 0 END), 0) AS expense
        FROM "JournalEntries"     e
        JOIN "JournalEntryLines"  l ON l."JournalEntryId" = e."Id"
        JOIN "Accounts"           a ON a."Id" = l."AccountId"
        WHERE e."Status" = 'Posted' AND e."Date" >= @from
        GROUP BY yr, mo
        ORDER BY yr, mo;
        """;

    public async Task<IReadOnlyList<TrialBalanceRow>> GetTrialBalanceAsync(CancellationToken cancellationToken = default)
    {
        await using var command = await CreateCommandAsync(TrialBalanceSql, cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var rows = new List<TrialBalanceRow>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rows.Add(new TrialBalanceRow(
                reader.GetString(0),
                reader.GetString(1),
                Enum.Parse<AccountType>(reader.GetString(2)),
                reader.GetDecimal(3),
                reader.GetDecimal(4)));
        }
        return rows;
    }

    public async Task<IReadOnlyList<CashFlowPoint>> GetCashFlowAsync(int months, CancellationToken cancellationToken = default)
    {
        if (months < 1) months = 6;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = new DateOnly(today.Year, today.Month, 1).AddMonths(-(months - 1));

        await using var command = await CreateCommandAsync(CashFlowSql, cancellationToken);
        AddParameter(command, "from", from);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var points = new List<CashFlowPoint>();
        while (await reader.ReadAsync(cancellationToken))
        {
            points.Add(new CashFlowPoint(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetDecimal(2),
                reader.GetDecimal(3)));
        }
        return points;
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var pendingCount = await _db.JournalEntries
            .CountAsync(e => e.Status == JournalEntryStatus.PendingReview, cancellationToken);

        var pendingHeld = await _db.JournalEntries
            .Where(e => e.Status == JournalEntryStatus.PendingReview)
            .SelectMany(e => e.Lines)
            .SumAsync(l => (decimal?)l.DebitAmount, cancellationToken) ?? 0m;

        var postedCount = await _db.JournalEntries
            .CountAsync(e => e.Status == JournalEntryStatus.Posted, cancellationToken);

        var autoPostedCount = await _db.JournalEntries
            .CountAsync(e => e.Status == JournalEntryStatus.Posted && e.ReviewedBy == "system (auto)", cancellationToken);

        return new DashboardSummary(pendingCount, pendingHeld, postedCount, autoPostedCount);
    }

    private async Task<DbCommand> CreateCommandAsync(string sql, CancellationToken cancellationToken)
    {
        var connection = _db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = sql;
        return command;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var p = command.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        command.Parameters.Add(p);
    }
}
