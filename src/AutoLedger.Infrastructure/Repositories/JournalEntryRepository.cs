using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoLedger.Infrastructure.Repositories;

public sealed class JournalEntryRepository : IJournalEntryRepository
{
    private readonly AppDbContext _db;

    public JournalEntryRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default)
        => await _db.JournalEntries.AddAsync(entry, cancellationToken);

    public Task<JournalEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _db.JournalEntries
            .Include(e => e.Vendor)
            .Include(e => e.Lines)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<PagedResult<JournalEntry>> GetPagedAsync(
        JournalEntryStatus? status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _db.JournalEntries.AsNoTracking();
        if (status is not null)
            query = query.Where(e => e.Status == status);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.Date)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(e => e.Vendor)
            .Include(e => e.Lines)
                .ThenInclude(l => l.Account)
            .ToListAsync(cancellationToken);

        return new PagedResult<JournalEntry>(items, total, page, pageSize);
    }

    public Task<int> CountByStatusAsync(JournalEntryStatus status, CancellationToken cancellationToken = default)
        => _db.JournalEntries.CountAsync(e => e.Status == status, cancellationToken);
}
