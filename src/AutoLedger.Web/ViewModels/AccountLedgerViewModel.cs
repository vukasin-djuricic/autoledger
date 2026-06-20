using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Reporting;

namespace AutoLedger.Web.ViewModels;

public sealed class AccountLedgerViewModel
{
    public required int AccountId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required AccountType Type { get; init; }
    public required IReadOnlyList<AccountLedgerRow> Rows { get; init; }

    public bool NormalBalanceIsDebit => Type is AccountType.Asset or AccountType.Expense;
    public decimal ClosingBalance => Rows.Count == 0 ? 0m : Rows[^1].RunningBalance;
}
