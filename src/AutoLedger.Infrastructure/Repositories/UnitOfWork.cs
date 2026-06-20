using AutoLedger.Domain.Abstractions;
using AutoLedger.Infrastructure.Persistence;

namespace AutoLedger.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db, IJournalEntryRepository journalEntries)
    {
        _db = db;
        JournalEntries = journalEntries;
    }

    public IJournalEntryRepository JournalEntries { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);

    public async Task<IDomainTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => new EfDomainTransaction(await _db.Database.BeginTransactionAsync(cancellationToken));
}
