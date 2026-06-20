using AutoLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoLedger.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Code).HasMaxLength(10).IsRequired();
        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(a => a.Code).IsUnique();
    }
}
