using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using Xunit;

namespace AutoLedger.Tests.Domain;

public class MasterDataTests
{
    [Fact]
    public void New_account_is_active_by_default()
        => Assert.True(new Account("5300", "Marketing Expense", AccountType.Expense).IsActive);

    [Fact]
    public void Account_can_be_renamed_deactivated_and_reactivated()
    {
        var account = new Account("5300", "Marketing", AccountType.Expense);

        account.Rename("Marketing Expense");
        Assert.Equal("Marketing Expense", account.Name);

        account.Deactivate();
        Assert.False(account.IsActive);

        account.Activate();
        Assert.True(account.IsActive);
    }

    [Fact]
    public void Account_rename_rejects_blank_name()
        => Assert.Throws<ArgumentException>(() => new Account("5300", "Marketing", AccountType.Expense).Rename("  "));

    [Fact]
    public void New_vendor_is_active_by_default()
        => Assert.True(new Vendor("Acme Corp").IsActive);

    [Fact]
    public void Vendor_can_be_renamed_and_deactivated()
    {
        var vendor = new Vendor("Acme");

        vendor.Rename("Acme Corp");
        Assert.Equal("Acme Corp", vendor.Name);

        vendor.Deactivate();
        Assert.False(vendor.IsActive);
    }

    [Fact]
    public void Vendor_rename_rejects_blank_name()
        => Assert.Throws<ArgumentException>(() => new Vendor("Acme").Rename(""));
}
