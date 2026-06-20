using AutoLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoLedger.Infrastructure.Persistence.Configurations;

public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("JournalEntries");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReferenceNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(256).IsRequired();
        builder.Property(e => e.ReviewedBy).HasMaxLength(256);
        builder.Property(e => e.RejectionReason).HasMaxLength(500);

        // Optimistic concurrency: PostgreSQL's system column xmin acts as the row version,
        // so two controllers can't approve the same entry over each other's change.
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasOne(e => e.Vendor)
            .WithMany()
            .HasForeignKey(e => e.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Lines are owned by the entry: read-only navigation backed by the _lines field.
        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata
            .FindNavigation(nameof(JournalEntry.Lines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Date);
        builder.HasIndex(e => e.ReferenceNumber);
    }
}
