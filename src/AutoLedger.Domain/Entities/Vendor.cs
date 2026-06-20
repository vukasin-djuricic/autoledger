namespace AutoLedger.Domain.Entities;

/// <summary>
/// A counterparty (supplier / payee). Journal entries can be linked to a vendor so
/// the risk engine can compare a new amount against that vendor's historical spend.
/// </summary>
public class Vendor
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Vendor() { } // EF

    public Vendor(string name) => Name = name;
}
