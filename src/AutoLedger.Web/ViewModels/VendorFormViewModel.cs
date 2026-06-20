using System.ComponentModel.DataAnnotations;

namespace AutoLedger.Web.ViewModels;

public sealed class VendorFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public bool IsEdit => Id is not null;
}
