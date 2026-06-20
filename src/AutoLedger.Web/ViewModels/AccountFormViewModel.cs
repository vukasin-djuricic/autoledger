using System.ComponentModel.DataAnnotations;
using AutoLedger.Domain.Enums;

namespace AutoLedger.Web.ViewModels;

public sealed class AccountFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(10)]
    [RegularExpression(@"^\d{3,10}$", ErrorMessage = "Code must be 3–10 digits.")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public AccountType Type { get; set; } = AccountType.Expense;

    public bool IsEdit => Id is not null;
}
