namespace AutoLedger.Domain.Reporting;

/// <summary>
/// One posted line in an account's ledger (drill-down from the Trial Balance), carrying a
/// running balance signed to the account's normal side.
/// </summary>
public sealed record AccountLedgerRow(
    int EntryId,
    DateOnly Date,
    string Reference,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance);
