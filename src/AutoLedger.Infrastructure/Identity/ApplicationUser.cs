using Microsoft.AspNetCore.Identity;

namespace AutoLedger.Infrastructure.Identity;

/// <summary>Application user. Extends the ASP.NET Identity user with a display name.</summary>
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? JobTitle { get; set; }
}

/// <summary>Centralised role names so controllers and seeding can't drift apart.</summary>
public static class Roles
{
    public const string Clerk = "Clerk";
    public const string Controller = "Controller";

    public static readonly string[] All = { Clerk, Controller };
}
