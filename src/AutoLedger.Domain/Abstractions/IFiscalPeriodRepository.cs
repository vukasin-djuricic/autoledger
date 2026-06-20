using AutoLedger.Domain.Entities;

namespace AutoLedger.Domain.Abstractions;

public interface IFiscalPeriodRepository
{
    /// <summary>The period covering a given date, or null if none is defined (treated as open).</summary>
    Task<FiscalPeriod?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default);

    Task<FiscalPeriod?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>All periods, newest first.</summary>
    Task<IReadOnlyList<FiscalPeriod>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(FiscalPeriod period, CancellationToken cancellationToken = default);
}
