using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using Xunit;

namespace AutoLedger.Tests.Domain;

public class FiscalPeriodTests
{
    [Fact]
    public void New_period_is_open()
        => Assert.Equal(FiscalPeriodStatus.Open, new FiscalPeriod(2026, 6).Status);

    [Fact]
    public void Close_and_reopen_toggle_status()
    {
        var period = new FiscalPeriod(2026, 6);

        period.Close();
        Assert.True(period.IsClosed);

        period.Reopen();
        Assert.False(period.IsClosed);
    }

    [Fact]
    public void Closing_an_already_closed_period_throws()
    {
        var period = new FiscalPeriod(2026, 6);
        period.Close();
        Assert.Throws<InvalidOperationException>(() => period.Close());
    }

    [Fact]
    public void Reopening_an_open_period_throws()
        => Assert.Throws<InvalidOperationException>(() => new FiscalPeriod(2026, 6).Reopen());

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void Month_must_be_valid(int month)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new FiscalPeriod(2026, month));
}
