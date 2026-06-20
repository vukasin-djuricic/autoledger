using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
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

    // ---- Test doubles ----

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public bool ThrowOnSave { get; init; }
        public bool SaveChangesCalled { get; private set; }
        public FakeTransaction? Transaction { get; private set; }

        public IJournalEntryRepository JournalEntries => throw new NotSupportedException();
        public IAccountRepository Accounts => throw new NotSupportedException();
        public IVendorRepository Vendors => throw new NotSupportedException();

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
}
