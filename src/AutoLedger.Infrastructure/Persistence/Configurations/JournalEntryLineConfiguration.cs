using AutoLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoLedger.Infrastructure.Persistence.Configurations;

public sealed class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.DebitAmount).HasPrecision(18, 2);
        builder.Property(l => l.CreditAmount).HasPrecision(18, 2);

        builder.HasOne(l => l.Account)
            .WithMany()
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.AccountId);
    }
}
