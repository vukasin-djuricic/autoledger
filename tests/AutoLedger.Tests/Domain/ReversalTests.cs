using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Exceptions;
using Xunit;

namespace AutoLedger.Tests.Domain;

public class ReversalTests
{
    private static JournalEntry PostedEntry()
    {
        var e = new JournalEntry(new DateOnly(2026, 6, 20), "JE-100", "Original entry", "clerk", vendorId: 7);
        e.AddLine(accountId: 1, debit: 100m, credit: 0m);
        e.AddLine(accountId: 2, debit: 0m, credit: 100m);
        e.Submit();
        e.Approve("controller");
        e.Post();
        return e;
    }

    [Fact]
    public void Reversal_swaps_debits_and_credits_and_stays_balanced()
    {
        var original = PostedEntry();

        var reversal = original.CreateReversal("controller");

        Assert.True(reversal.IsBalanced);
        Assert.Equal(original.TotalDebit, reversal.TotalCredit);
        Assert.Equal(original.TotalCredit, reversal.TotalDebit);

        var origLine = original.Lines.First(l => l.DebitAmount > 0);
        var revLine = reversal.Lines.First(l => l.AccountId == origLine.AccountId);
        Assert.Equal(0m, revLine.DebitAmount);
        Assert.Equal(origLine.DebitAmount, revLine.CreditAmount);
    }

    [Fact]
    public void Reversal_is_linked_to_the_original_in_both_directions()
    {
        var original = PostedEntry();

        var reversal = original.CreateReversal("controller");

        Assert.True(reversal.IsReversal);
        Assert.Same(original, reversal.ReversalOf);
        Assert.True(original.IsReversed);
        Assert.Same(reversal, original.ReversedBy);
        Assert.Equal("REV-JE-100", reversal.ReferenceNumber);
        Assert.Equal(original.VendorId, reversal.VendorId);
        Assert.Equal(JournalEntryStatus.Draft, reversal.Status); // not yet pushed through the workflow
    }

    [Fact]
    public void Cannot_reverse_an_entry_that_is_not_posted()
    {
        var draft = new JournalEntry(new DateOnly(2026, 6, 20), "JE-200", "Draft", "clerk");
        draft.AddLine(1, 50m, 0m);
        draft.AddLine(2, 0m, 50m);

        Assert.Throws<InvalidTransitionException>(() => draft.CreateReversal("controller"));
    }

    [Fact]
    public void Cannot_reverse_the_same_entry_twice()
    {
        var original = PostedEntry();
        original.CreateReversal("controller");

        Assert.Throws<InvalidOperationException>(() => original.CreateReversal("controller"));
    }
}
