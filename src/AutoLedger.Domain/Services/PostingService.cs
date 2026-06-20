using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Exceptions;

namespace AutoLedger.Domain.Services;

/// <summary>
/// Posts an approved entry to the ledger inside an explicit database transaction.
/// This is the Unit-of-Work / ACID demonstration: the balance is re-validated, the status
/// is advanced to Posted, and the change is committed atomically — any failure rolls the
/// whole operation back so the ledger is never left in a half-written state. Posting into a
/// closed fiscal period is rejected.
/// </summary>
public sealed class PostingService
{
    private readonly IUnitOfWork _unitOfWork;

    public PostingService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task PostAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        entry.EnsureBalanced(); // never post an unbalanced entry

        // A period is locked only when it exists and is closed; an undefined period is treated as open.
        var period = await _unitOfWork.FiscalPeriods.GetByDateAsync(entry.Date, cancellationToken);
        if (period is { Status: FiscalPeriodStatus.Closed })
            throw new ClosedPeriodException(entry.Date);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            entry.Post(); // Approved -> Posted (guarded by the State pattern)
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
