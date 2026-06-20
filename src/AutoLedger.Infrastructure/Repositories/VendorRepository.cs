using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoLedger.Infrastructure.Repositories;

public sealed class VendorRepository : IVendorRepository
{
    private readonly AppDbContext _db;

    public VendorRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Vendor>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _db.Vendors.AsNoTracking();
        if (!includeInactive)
            query = query.Where(v => v.IsActive);
        return await query.OrderBy(v => v.Name).ToListAsync(cancellationToken);
    }

    public Task<Vendor?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _db.Vendors.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task AddAsync(Vendor vendor, CancellationToken cancellationToken = default)
        => await _db.Vendors.AddAsync(vendor, cancellationToken);
}
