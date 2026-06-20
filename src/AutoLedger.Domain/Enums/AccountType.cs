namespace AutoLedger.Domain.Enums;

/// <summary>
/// The five fundamental account categories of double-entry accounting.
/// Determines the "normal" balance side of an account (debit vs credit).
/// </summary>
public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}
