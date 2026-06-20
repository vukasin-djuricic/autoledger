namespace AutoLedger.Domain.Entities;

/// <summary>
/// A counterparty (supplier / payee). Journal entries can be linked to a vendor so
/// the risk engine can compare a new amount against that vendor's historical spend.
/// </summary>
public class Vendor
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    /// <summary>Inactive vendors are hidden from new entries but kept for posted history.</summary>
    public bool IsActive { get; private set; } = true;

    private Vendor() { } // EF

    public Vendor(string name) => Name = name;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Vendor name is required.", nameof(name));
        Name = name.Trim();
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
