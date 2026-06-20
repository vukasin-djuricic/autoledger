using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoLedger.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly AppDbContext _db;

    public AccountRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Account>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _db.Accounts.AsNoTracking();
        if (!includeInactive)
            query = query.Where(a => a.IsActive);
        return await query.OrderBy(a => a.Code).ToListAsync(cancellationToken);
    }

    public Task<Account?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _db.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
        => _db.Accounts.AnyAsync(a => a.Code == code, cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
        => await _db.Accounts.AddAsync(account, cancellationToken);
}
