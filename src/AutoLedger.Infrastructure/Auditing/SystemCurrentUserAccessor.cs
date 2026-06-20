using AutoLedger.Domain.Abstractions;

namespace AutoLedger.Infrastructure.Auditing;

/// <summary>
/// Default accessor used for background/seed operations. The web layer replaces this with an
/// HTTP-aware implementation that reads the signed-in user.
/// </summary>
public sealed class SystemCurrentUserAccessor : ICurrentUserAccessor
{
    public string? UserName => "system";
}
