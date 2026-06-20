using AutoLedger.Domain.Abstractions;

namespace AutoLedger.Web.Services;

/// <summary>Reads the signed-in user from the current HTTP request for the audit trail.</summary>
public sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
}
