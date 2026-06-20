using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Exceptions;
using AutoLedger.Domain.Services;
using Xunit;

namespace AutoLedger.Tests.Domain;

public class PostingServiceTests
{
    private static JournalEntry ApprovedEntry()
    {
        var e = new JournalEntry(new DateOnly(2026, 6, 20), "JE-POST-1", "Posting test", "tester");
        e.AddLine(1, 100m, 0m);
        e.AddLine(2, 0m, 100m);
        e.Submit();
        e.Approve("controller");
        return e;
    }

    [Fact]
    public async Task Posting_commits_and_advances_to_posted()
    {
        var uow = new FakeUnitOfWork();
        var entry = ApprovedEntry();

        await new PostingService(uow).PostAsync(entry);

        Assert.Equal(JournalEntryStatus.Posted, entry.Status);
        Assert.True(uow.SaveChangesCalled);
        Assert.True(uow.Transaction!.Committed);
        Assert.False(uow.Transaction!.RolledBack);
    }

    [Fact]
    public async Task Posting_rolls_back_when_save_fails()
    {
        var uow = new FakeUnitOfWork { ThrowOnSave = true };
        var entry = ApprovedEntry();

        await Assert.ThrowsAsync<InvalidOperationException>(() => new PostingService(uow).PostAsync(entry));

        Assert.True(uow.Transaction!.RolledBack);
        Assert.False(uow.Transaction!.Committed);
    }

    [Fact]
    public async Task Posting_into_a_closed_period_is_rejected()
    {
        var closed = new FiscalPeriod(2026, 6);
        closed.Close();
        var uow = new FakeUnitOfWork { Period = closed };
        var entry = ApprovedEntry(); // dated 2026-06-20

        await Assert.ThrowsAsync<ClosedPeriodException>(() => new PostingService(uow).PostAsync(entry));

        Assert.Equal(JournalEntryStatus.Approved, entry.Status); // never advanced to Posted
        Assert.Null(uow.Transaction); // guarded before the transaction opened
    }

    [Fact]
    public async Task Posting_into_an_open_period_succeeds()
    {
        var uow = new FakeUnitOfWork { Period = new FiscalPeriod(2026, 6) }; // open
        var entry = ApprovedEntry();

        await new PostingService(uow).PostAsync(entry);

        Assert.Equal(JournalEntryStatus.Posted, entry.Status);
    }

    // ---- Test doubles ----

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public bool ThrowOnSave { get; init; }
        public FiscalPeriod? Period { get; init; } // null => period undefined (treated as open)
        public bool SaveChangesCalled { get; private set; }
        public FakeTransaction? Transaction { get; private set; }

        public IJournalEntryRepository JournalEntries => throw new NotSupportedException();
        public IAccountRepository Accounts => throw new NotSupportedException();
        public IVendorRepository Vendors => throw new NotSupportedException();
        public IFiscalPeriodRepository FiscalPeriods => new FakeFiscalPeriodRepository(Period);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalled = true;
            if (ThrowOnSave) throw new InvalidOperationException("save failed");
            return Task.FromResult(1);
        }

        public Task<IDomainTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            Transaction = new FakeTransaction();
            return Task.FromResult<IDomainTransaction>(Transaction);
        }
    }

    private sealed class FakeTransaction : IDomainTransaction
    {
        public bool Committed { get; private set; }
        public bool RolledBack { get; private set; }

        public Task CommitAsync(CancellationToken cancellationToken = default) { Committed = true; return Task.CompletedTask; }
        public Task RollbackAsync(CancellationToken cancellationToken = default) { RolledBack = true; return Task.CompletedTask; }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeFiscalPeriodRepository : IFiscalPeriodRepository
    {
        private readonly FiscalPeriod? _period;
        public FakeFiscalPeriodRepository(FiscalPeriod? period) => _period = period;

        public Task<FiscalPeriod?> GetByDateAsync(DateOnly date, CancellationToken cancellationToken = default)
            => Task.FromResult(_period);

        public Task<FiscalPeriod?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult(_period);

        public Task<IReadOnlyList<FiscalPeriod>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<FiscalPeriod>>(_period is null ? Array.Empty<FiscalPeriod>() : new[] { _period });

        public Task AddAsync(FiscalPeriod period, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
