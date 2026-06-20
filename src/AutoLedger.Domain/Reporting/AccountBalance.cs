using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.Reporting;

/// <summary>An account's net balance (positive on its normal side) — used to build closing entries.</summary>
public sealed record AccountBalance(int AccountId, AccountType Type, decimal Amount);
