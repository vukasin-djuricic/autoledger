using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;

namespace AutoLedger.Web.ViewModels;

public sealed class TransactionsIndexViewModel
{
    public required PagedResult<JournalEntry> Page { get; init; }
    public JournalEntryStatus? Status { get; init; }

    public static readonly (string Label, JournalEntryStatus? Value)[] Filters =
    {
        ("All", null),
        ("Draft", JournalEntryStatus.Draft),
        ("Pending", JournalEntryStatus.PendingReview),
        ("Approved", JournalEntryStatus.Approved),
        ("Posted", JournalEntryStatus.Posted),
        ("Rejected", JournalEntryStatus.Rejected),
    };
}
