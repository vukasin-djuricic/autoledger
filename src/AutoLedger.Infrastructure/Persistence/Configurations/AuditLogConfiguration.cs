using AutoLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoLedger.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.EntityName).HasMaxLength(128).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(64).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(32).IsRequired();
        builder.Property(a => a.UserId).HasMaxLength(256);
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.Timestamp);
    }
}
