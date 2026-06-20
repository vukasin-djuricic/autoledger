namespace AutoLedger.Domain.Entities;

/// <summary>
/// An immutable record of a change to a tracked entity. Written automatically by the
/// EF Core SaveChanges interceptor in the infrastructure layer.
/// </summary>
public class AuditLog
{
    public long Id { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty; // Created, Modified, StatusChanged, Deleted
    public string? Details { get; private set; }
    public string? UserId { get; private set; }
    public DateTime Timestamp { get; private set; }

    private AuditLog() { } // EF

    public AuditLog(string entityName, string entityId, string action, string? details, string? userId)
    {
        EntityName = entityName;
        EntityId = entityId;
        Action = action;
        Details = details;
        UserId = userId;
        Timestamp = DateTime.UtcNow;
    }
}
