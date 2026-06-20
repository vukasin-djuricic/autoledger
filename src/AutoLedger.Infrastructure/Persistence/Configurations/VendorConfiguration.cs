using AutoLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoLedger.Infrastructure.Persistence.Configurations;

public sealed class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(v => v.Name);
    }
}
