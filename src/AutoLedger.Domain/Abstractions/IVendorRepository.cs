using AutoLedger.Domain.Entities;

namespace AutoLedger.Domain.Abstractions;

public interface IVendorRepository
{
    /// <summary>Vendors ordered by name. Inactive vendors are included only when asked.</summary>
    Task<IReadOnlyList<Vendor>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default);

    Task<Vendor?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default);
}
