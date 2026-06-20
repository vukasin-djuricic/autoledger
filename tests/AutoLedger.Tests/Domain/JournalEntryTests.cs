using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Exceptions;
using Xunit;

namespace AutoLedger.Tests.Domain;

public class JournalEntryTests
{
    private static JournalEntry BalancedDraft()
    {
        var e = new JournalEntry(new DateOnly(2026, 6, 20), "JE-TEST-1", "Test entry", "tester");
        e.AddLine(accountId: 1, debit: 100m, credit: 0m);
        e.AddLine(accountId: 2, debit: 0m, credit: 100m);
        return e;
    }

    // ---- Double-entry invariant -------------------------------------------------

    [Fact]
    public void Balanced_entry_passes_validation()
    {
        var entry = BalancedDraft();
        Assert.True(entry.IsBalanced);
        entry.EnsureBalanced(); // does not throw
    }

    [Fact]
    public void Unbalanced_entry_throws_with_both_totals()
    {
        var entry = new JournalEntry(new DateOnly(2026, 6, 20), "JE-TEST-2", "Bad", "tester");
        entry.AddLine(1, 100m, 0m);
        entry.AddLine(2, 0m, 60m);

        var ex = Assert.Throws<UnbalancedEntryException>(() => entry.EnsureBalanced());
        Assert.Equal(100m, ex.TotalDebit);
        Assert.Equal(60m, ex.TotalCredit);
    }

    [Fact]
    public void Single_line_is_not_balanced()
    {
        var entry = new JournalEntry(new DateOnly(2026, 6, 20), "JE-TEST-3", "One line", "tester");
        entry.AddLine(1, 100m, 0m);
        Assert.False(entry.IsBalanced);
    }

    [Theory]
    [InlineData(50, 50)]   // both sides set
    [InlineData(0, 0)]     // neither side set
    public void Line_must_be_exactly_debit_or_credit(decimal debit, decimal credit)
        => Assert.Throws<ArgumentException>(() => new JournalEntryLine(1, debit, credit));

    // ---- State machine: legal path ---------------------------------------------

    [Fact]
    public void Full_lifecycle_draft_to_posted()
    {
        var entry = BalancedDraft();
        Assert.Equal(JournalEntryStatus.Draft, entry.Status);

        entry.Submit();
        Assert.Equal(JournalEntryStatus.PendingReview, entry.Status);

        entry.Approve("controller");
        Assert.Equal(JournalEntryStatus.Approved, entry.Status);
        Assert.Equal("controller", entry.ReviewedBy);

        entry.Post();
        Assert.Equal(JournalEntryStatus.Posted, entry.Status);
    }

    [Fact]
    public void Reject_sets_reason_and_terminal_state()
    {
        var entry = BalancedDraft();
        entry.Submit();
        entry.Reject("controller", "Missing documentation");

        Assert.Equal(JournalEntryStatus.Rejected, entry.Status);
        Assert.Equal("Missing documentation", entry.RejectionReason);
    }

    // ---- State machine: illegal transitions ------------------------------------

    [Fact]
    public void Cannot_approve_a_draft()
        => Assert.Throws<InvalidTransitionException>(() => BalancedDraft().Approve("controller"));

    [Fact]
    public void Cannot_post_before_approval()
    {
        var entry = BalancedDraft();
        entry.Submit();
        Assert.Throws<InvalidTransitionException>(() => entry.Post());
    }

    [Fact]
    public void Cannot_submit_an_unbalanced_draft()
    {
        var entry = new JournalEntry(new DateOnly(2026, 6, 20), "JE-TEST-4", "Bad", "tester");
        entry.AddLine(1, 100m, 0m);
        entry.AddLine(2, 0m, 99m);
        Assert.Throws<UnbalancedEntryException>(() => entry.Submit());
    }

    [Fact]
    public void Posted_entry_is_immutable()
    {
        var entry = BalancedDraft();
        entry.Submit();
        entry.Approve("controller");
        entry.Post();

        Assert.Throws<InvalidTransitionException>(() => entry.AddLine(3, 10m, 0m));
    }

    [Fact]
    public void Rejection_requires_a_reason()
    {
        var entry = BalancedDraft();
        entry.Submit();
        Assert.Throws<ArgumentException>(() => entry.Reject("controller", "   "));
    }
}
