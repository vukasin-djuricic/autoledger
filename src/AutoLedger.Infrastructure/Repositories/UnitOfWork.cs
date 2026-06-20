using AutoLedger.Domain.Abstractions;
using AutoLedger.Infrastructure.Persistence;

namespace AutoLedger.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db, IJournalEntryRepository journalEntries,
        IAccountRepository accounts, IVendorRepository vendors, IFiscalPeriodRepository fiscalPeriods)
    {
        _db = db;
        JournalEntries = journalEntries;
        Accounts = accounts;
        Vendors = vendors;
        FiscalPeriods = fiscalPeriods;
    }

    public IJournalEntryRepository JournalEntries { get; }
    public IAccountRepository Accounts { get; }
    public IVendorRepository Vendors { get; }
    public IFiscalPeriodRepository FiscalPeriods { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);

    public async Task<IDomainTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => new EfDomainTransaction(await _db.Database.BeginTransactionAsync(cancellationToken));
}
