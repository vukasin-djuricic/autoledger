using AutoLedger.Domain.Enums;
using AutoLedger.Domain.Reporting;
using AutoLedger.Domain.Services;
using Xunit;

namespace AutoLedger.Tests.Domain;

public class YearEndCloseTests
{
    private const int RetainedEarningsId = 99;

    [Fact]
    public void Closing_entry_is_balanced_and_posts_net_profit_to_retained_earnings()
    {
        var balances = new List<AccountBalance>
        {
            new(AccountId: 1, AccountType.Revenue, Amount: 1_000m),
            new(AccountId: 2, AccountType.Expense, Amount: 300m),
        };

        var entry = YearEndCloseService.BuildClosingEntry(2026, "YEC-2026", "controller", balances, RetainedEarningsId);

        Assert.True(entry.IsBalanced);

        // Revenue (1000) is debited to clear it; expense (300) credited; net 700 credited to equity.
        var reLine = entry.Lines.Single(l => l.AccountId == RetainedEarningsId);
        Assert.Equal(700m, reLine.CreditAmount);
        Assert.Equal(0m, reLine.DebitAmount);
    }

    [Fact]
    public void A_net_loss_is_debited_to_retained_earnings()
    {
        var balances = new List<AccountBalance>
        {
            new(AccountId: 1, AccountType.Revenue, Amount: 400m),
            new(AccountId: 2, AccountType.Expense, Amount: 1_000m),
        };

        var entry = YearEndCloseService.BuildClosingEntry(2026, "YEC-2026", "controller", balances, RetainedEarningsId);

        Assert.True(entry.IsBalanced);
        var reLine = entry.Lines.Single(l => l.AccountId == RetainedEarningsId);
        Assert.Equal(600m, reLine.DebitAmount); // loss reduces equity
        Assert.Equal(0m, reLine.CreditAmount);
    }

    [Fact]
    public void Closing_entry_is_dated_at_year_end()
    {
        var balances = new List<AccountBalance> { new(1, AccountType.Revenue, 100m), new(2, AccountType.Expense, 100m) };

        var entry = YearEndCloseService.BuildClosingEntry(2026, "YEC-2026", "controller", balances, RetainedEarningsId);

        Assert.Equal(new DateOnly(2026, 12, 31), entry.Date);
    }
}
