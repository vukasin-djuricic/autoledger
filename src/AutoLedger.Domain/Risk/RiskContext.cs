namespace AutoLedger.Domain.Risk;

/// <summary>Inputs a risk strategy evaluates: the new amount and the vendor's history.</summary>
/// <param name="Amount">The monetary size of the entry (total debit).</param>
/// <param name="HasVendor">Whether the entry is linked to a known vendor.</param>
/// <param name="VendorHistory">Statistics over the vendor's prior posted entries.</param>
public readonly record struct RiskContext(decimal Amount, bool HasVendor, VendorStatistics VendorHistory);
