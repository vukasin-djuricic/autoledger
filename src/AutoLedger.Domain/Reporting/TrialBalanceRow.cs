using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.Reporting;

/// <summary>One row of the Trial Balance report: an account with its net debit/credit totals.</summary>
public sealed record TrialBalanceRow(
    int AccountId,
    string AccountCode,
    string AccountName,
    AccountType AccountType,
    decimal Debit,
    decimal Credit);
