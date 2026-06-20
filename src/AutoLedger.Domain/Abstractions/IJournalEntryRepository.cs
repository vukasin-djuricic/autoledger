using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;

namespace AutoLedger.Domain.Abstractions;

/// <summary>Persistence operations for the <see cref="JournalEntry"/> aggregate.</summary>
public interface IJournalEntryRepository
{
    Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Loads an entry with its lines (and each line's account).</summary>
    Task<JournalEntry?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>A page of entries for the grid, optionally filtered by status, newest first.</summary>
    Task<PagedResult<JournalEntry>> GetPagedAsync(
        JournalEntryStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountByStatusAsync(JournalEntryStatus status, CancellationToken cancellationToken = default);
}

/// <summary>A page of results plus the total count for pagination UI.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
