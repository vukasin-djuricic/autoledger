namespace AutoLedger.Domain.Abstractions;

/// <summary>
/// An explicit database transaction, abstracted so the domain can demonstrate
/// commit/rollback semantics (ACID) without depending on EF Core.
/// </summary>
public interface IDomainTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
