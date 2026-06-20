using AutoLedger.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace AutoLedger.Infrastructure.Repositories;

/// <summary>Adapts an EF Core transaction to the domain's <see cref="IDomainTransaction"/>.</summary>
public sealed class EfDomainTransaction : IDomainTransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfDomainTransaction(IDbContextTransaction transaction) => _transaction = transaction;

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => _transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => _transaction.RollbackAsync(cancellationToken);

    public ValueTask DisposeAsync() => _transaction.DisposeAsync();
}
