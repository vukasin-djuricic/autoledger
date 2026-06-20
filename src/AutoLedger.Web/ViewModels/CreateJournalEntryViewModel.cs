using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoLedger.Web.ViewModels;

public sealed class CreateJournalEntryViewModel
{
    [DataType(DataType.Date)]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [Required, StringLength(50)]
    public string Reference { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public int? VendorId { get; set; }

    public List<LineInput> Lines { get; set; } = new()
    {
        new LineInput(), new LineInput()
    };

    // Populated by the controller for the dropdowns.
    public IReadOnlyList<SelectListItem> AccountOptions { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> VendorOptions { get; set; } = Array.Empty<SelectListItem>();
}

public sealed class LineInput
{
    public int? AccountId { get; set; }
    public decimal? Debit { get; set; }
    public decimal? Credit { get; set; }
}
