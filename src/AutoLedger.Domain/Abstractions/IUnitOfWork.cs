namespace AutoLedger.Domain.Abstractions;

/// <summary>
/// Unit of Work: groups repository operations under one transactional boundary and a
/// single SaveChanges. Exposes explicit transactions for multi-step operations that
/// must be all-or-nothing (e.g. posting a journal entry).
/// </summary>
public interface IUnitOfWork
{
    IJournalEntryRepository JournalEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDomainTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
