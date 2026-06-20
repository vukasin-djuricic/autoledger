using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AutoLedger.Infrastructure.Auditing;

/// <summary>
/// Writes an <see cref="AuditLog"/> row for every change to a business entity, automatically,
/// on each SaveChanges. Status changes on a <see cref="JournalEntry"/> are recorded as
/// "StatusChanged" with the before/after value — giving a tamper-evident history for free.
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<Type> Audited = new()
    {
        typeof(Account), typeof(Vendor), typeof(JournalEntry), typeof(JournalEntryLine), typeof(FiscalPeriod)
    };

    private readonly ICurrentUserAccessor _currentUser;

    public AuditSaveChangesInterceptor(ICurrentUserAccessor currentUser) => _currentUser = currentUser;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(DbContext? context)
    {
        if (context is null) return;

        var user = _currentUser.UserName;

        // Snapshot first — we must not mutate the change tracker while enumerating it.
        var auditable = context.ChangeTracker.Entries()
            .Where(e => Audited.Contains(e.Entity.GetType())
                        && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        var logs = new List<AuditLog>();
        foreach (var entry in auditable)
        {
            var entityName = entry.Entity.GetType().Name;
            var entityId = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())
                ?.CurrentValue?.ToString() ?? "?";

            var (action, details) = Describe(entry);
            logs.Add(new AuditLog(entityName, entityId, action, details, user));
        }

        if (logs.Count > 0)
            context.Set<AuditLog>().AddRange(logs);
    }

    private static (string Action, string? Details) Describe(EntityEntry entry)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                return ("Created", null);

            case EntityState.Deleted:
                return ("Deleted", null);

            case EntityState.Modified:
                if (entry.Entity is JournalEntry)
                {
                    var status = entry.Property(nameof(JournalEntry.Status));
                    if (status.IsModified && !Equals(status.OriginalValue, status.CurrentValue))
                    {
                        return ("StatusChanged",
                            $"{(JournalEntryStatus)status.OriginalValue!} → {(JournalEntryStatus)status.CurrentValue!}");
                    }
                }
                return ("Modified", null);

            default:
                return ("Modified", null);
        }
    }
}
