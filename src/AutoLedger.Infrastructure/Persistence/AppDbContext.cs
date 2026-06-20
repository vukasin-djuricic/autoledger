using AutoLedger.Domain.Entities;
using AutoLedger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoLedger.Infrastructure.Persistence;

/// <summary>
/// EF Core context. Also acts as the Identity store (extends <see cref="IdentityDbContext{TUser}"/>),
/// so application data and authentication live in one PostgreSQL database / one transaction scope.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity tables
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // All money columns get a consistent precision/scale.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }
}
