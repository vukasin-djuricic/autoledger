using AutoLedger.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace AutoLedger.Web.Components;

/// <summary>Renders the colour-coded status pill used across the grid, dashboard and details.</summary>
public sealed class StatusBadgeViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(JournalEntryStatus status)
        => View(StatusBadgeModel.For(status));
}

public sealed record StatusBadgeModel(string Label, string PillClasses, string DotClasses)
{
    public static StatusBadgeModel For(JournalEntryStatus status) => status switch
    {
        JournalEntryStatus.Draft         => new("Draft", "bg-stone-100 text-stone-500", "bg-stone-400"),
        JournalEntryStatus.PendingReview => new("Pending Review", "bg-amber-50 text-amber-700", "bg-amber-500"),
        JournalEntryStatus.Approved      => new("Approved", "bg-emerald-50 text-emerald-700", "bg-emerald-500"),
        JournalEntryStatus.Posted        => new("Posted", "bg-brand-50 text-brand-700", "bg-brand-600"),
        JournalEntryStatus.Rejected      => new("Rejected", "bg-rose-50 text-rose-700", "bg-rose-500"),
        _ => new(status.ToString(), "bg-stone-100 text-stone-500", "bg-stone-400")
    };
}
