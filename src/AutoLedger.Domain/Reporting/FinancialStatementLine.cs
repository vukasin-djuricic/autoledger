using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.Reporting;

/// <summary>
/// One account line in a financial statement (Income Statement or Balance Sheet), with its net
/// amount expressed as a positive number on the account's normal side.
/// </summary>
public sealed record FinancialStatementLine(
    string AccountCode,
    string AccountName,
    AccountType Type,
    decimal Amount);
