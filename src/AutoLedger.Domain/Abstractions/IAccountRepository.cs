using AutoLedger.Domain.Entities;

namespace AutoLedger.Domain.Abstractions;

public interface IAccountRepository
{
    /// <summary>Chart of accounts, ordered by code. Inactive accounts are included only when asked.</summary>
    Task<IReadOnlyList<Account>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default);

    Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>True if an account with this code already exists (codes are unique).</summary>
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);

    Task AddAsync(Account account, CancellationToken cancellationToken = default);
}
