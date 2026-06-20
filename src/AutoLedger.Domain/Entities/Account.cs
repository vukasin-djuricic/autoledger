using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.Entities;

/// <summary>
/// A ledger account (a line in the Chart of Accounts), e.g. "1110 — Bank — Operating".
/// </summary>
public class Account
{
    public int Id { get; private set; }

    /// <summary>Numeric account code, e.g. "5200".</summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public AccountType Type { get; private set; }

    /// <summary>Inactive accounts are hidden from new entries but kept for posted history.</summary>
    public bool IsActive { get; private set; } = true;

    private Account() { } // EF

    public Account(string code, string name, AccountType type)
    {
        Code = code;
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Assets and Expenses increase on the debit side; Liabilities, Equity and
    /// Revenue increase on the credit side. Used by reporting (Trial Balance).
    /// </summary>
    public bool NormalBalanceIsDebit =>
        Type is AccountType.Asset or AccountType.Expense;

    /// <summary>Renames the account. The code and type stay fixed once created.</summary>
    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name is required.", nameof(name));
        Name = name.Trim();
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
