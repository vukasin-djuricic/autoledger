namespace AutoLedger.Domain.Abstractions;

/// <summary>Exposes the identity of the acting user to lower layers (e.g. the audit interceptor).</summary>
public interface ICurrentUserAccessor
{
    /// <summary>The current user's name/email, or null for system/background operations.</summary>
    string? UserName { get; }
}
