using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Exceptions;
using Xunit;

namespace AutoLedger.Tests.Domain;

public class EditResubmitTests
{
    private static JournalEntry RejectedEntry()
    {
        var e = new JournalEntry(new DateOnly(2026, 6, 20), "JE-300", "To fix", "clerk");
        e.AddLine(1, 100m, 0m);
        e.AddLine(2, 0m, 100m);
        e.Submit();
        e.Reject("controller", "Missing documentation");
        return e;
    }

    [Fact]
    public void Rejected_entry_reopens_to_draft_and_clears_review()
    {
        var entry = RejectedEntry();

        entry.Reopen();

        Assert.Equal(JournalEntryStatus.Draft, entry.Status);
        Assert.Null(entry.ReviewedBy);
        Assert.Null(entry.ReviewedAt);
        Assert.Null(entry.RejectionReason);
    }

    [Fact]
    public void Reopened_entry_can_be_edited_and_resubmitted()
    {
        var entry = RejectedEntry();
        entry.Reopen();

        entry.UpdateHeader(new DateOnly(2026, 6, 21), "JE-300A", "Corrected", vendorId: 5);
        entry.ClearLines();
        entry.AddLine(1, 250m, 0m);
        entry.AddLine(2, 0m, 250m);

        entry.Submit(); // resubmit through the workflow

        Assert.Equal(JournalEntryStatus.PendingReview, entry.Status);
        Assert.Equal("JE-300A", entry.ReferenceNumber);
        Assert.Equal(5, entry.VendorId);
        Assert.Equal(250m, entry.TotalDebit);
    }

    [Fact]
    public void Cannot_reopen_an_entry_that_is_not_rejected()
    {
        var draft = new JournalEntry(new DateOnly(2026, 6, 20), "JE-301", "Draft", "clerk");
        Assert.Throws<InvalidTransitionException>(() => draft.Reopen());
    }

    [Fact]
    public void Cannot_edit_a_posted_entry()
    {
        var entry = new JournalEntry(new DateOnly(2026, 6, 20), "JE-302", "Posted", "clerk");
        entry.AddLine(1, 100m, 0m);
        entry.AddLine(2, 0m, 100m);
        entry.Submit();
        entry.Approve("controller");
        entry.Post();

        Assert.Throws<InvalidTransitionException>(() => entry.UpdateHeader(new DateOnly(2026, 6, 21), "X", "Y", null));
        Assert.Throws<InvalidTransitionException>(() => entry.ClearLines());
    }
}
