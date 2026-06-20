using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoLedger.Infrastructure.Repositories;

public sealed class FiscalPeriodRepository : IFiscalPeriodRepository
{
    private readonly AppDbContext _db;

    public FiscalPeriodRepository(AppDbContext db) => _db = db;

    public Task<FiscalPeriod?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
        => _db.FiscalPeriods.FirstOrDefaultAsync(p => p.Year == date.Year && p.Month == date.Month, cancellationToken);

    public Task<FiscalPeriod?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _db.FiscalPeriods.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<FiscalPeriod>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _db.FiscalPeriods.AsNoTracking()
            .OrderByDescending(p => p.Year).ThenByDescending(p => p.Month)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(FiscalPeriod period, CancellationToken cancellationToken = default)
        => await _db.FiscalPeriods.AddAsync(period, cancellationToken);
}
