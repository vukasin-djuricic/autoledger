using AutoLedger.Domain.Entities;
using AutoLedger.Domain.Enums;
using AutoLedger.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoLedger.Infrastructure.Persistence;

/// <summary>
/// Applies migrations and populates the database with a realistic chart of accounts, vendors,
/// demo users/roles, and ~80 journal entries spread across statuses and months — enough for the
/// dashboard charts and the vendor-deviation risk engine to have meaningful data.
/// </summary>
public static class DbSeeder
{
    public const string ControllerEmail = "controller@autoledger.local";
    public const string ClerkEmail = "clerk@autoledger.local";
    public const string DemoPassword = "Passw0rd!";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(cancellationToken);

        await SeedRolesAndUsersAsync(services);

        if (await db.Accounts.AnyAsync(cancellationToken))
            return; // already seeded

        var accounts = SeedAccounts(db);
        await db.SaveChangesAsync(cancellationToken);

        var vendors = SeedVendors(db);
        await db.SaveChangesAsync(cancellationToken);

        SeedJournalEntries(db, accounts, vendors);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRolesAndUsersAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await EnsureUserAsync(userManager, ControllerEmail, "Jelena Marić", "Controller", Roles.Controller);
        await EnsureUserAsync(userManager, ClerkEmail, "Marko Petrović", "Accounting Clerk", Roles.Clerk);
    }

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager, string email, string displayName, string jobTitle, string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = displayName,
            JobTitle = jobTitle
        };
        var result = await userManager.CreateAsync(user, DemoPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }

    private static Dictionary<string, Account> SeedAccounts(AppDbContext db)
    {
        var accounts = new[]
        {
            new Account("1110", "Bank — Operating", AccountType.Asset),
            new Account("1200", "Accounts Receivable", AccountType.Asset),
            new Account("1450", "Input VAT Receivable", AccountType.Asset),
            new Account("1700", "Accumulated Depreciation", AccountType.Asset),
            new Account("2100", "Accounts Payable", AccountType.Liability),
            new Account("2300", "Output VAT Payable", AccountType.Liability),
            new Account("4000", "Sales Revenue", AccountType.Revenue),
            new Account("5100", "Rent Expense", AccountType.Expense),
            new Account("5200", "Office Expenses", AccountType.Expense),
            new Account("6000", "Salaries Payable", AccountType.Liability),
        };
        db.Accounts.AddRange(accounts);
        return accounts.ToDictionary(a => a.Code);
    }

    private static Dictionary<string, Vendor> SeedVendors(AppDbContext db)
    {
        var vendors = new[]
        {
            new Vendor("Hardware Supplies Ltd"),
            new Vendor("CloudHost Services"),
            new Vendor("Office Depot"),
            new Vendor("Belgrade HQ Landlord"),
            new Vendor("Acme Corp"),
            new Vendor("Globex EU Subsidiary"),
        };
        db.Vendors.AddRange(vendors);
        return vendors.ToDictionary(v => v.Name);
    }

    private static void SeedJournalEntries(
        AppDbContext db, Dictionary<string, Account> acc, Dictionary<string, Vendor> ven)
    {
        var rng = new Random(20260619);
        var refSeq = 4300;
        string NextRef() => $"JE-2026-0{refSeq++}";

        // ---- Posted expense history per vendor (stable means → usable statistics) ----
        var vendorProfiles = new (string Vendor, string ExpenseAccount, decimal Mean, decimal Spread)[]
        {
            ("Hardware Supplies Ltd", "5200", 5_000m, 800m),
            ("CloudHost Services",    "5200", 1_200m, 150m),
            ("Office Depot",          "5200",   480m,  90m),
            ("Belgrade HQ Landlord",  "5100", 9_500m, 200m),
        };

        foreach (var profile in vendorProfiles)
        {
            for (var i = 0; i < 8; i++)
            {
                var month = 1 + (i % 6);
                var day = 1 + rng.Next(0, 27);
                var amount = Math.Round(profile.Mean + (decimal)(rng.NextDouble() * 2 - 1) * profile.Spread, 2);

                var e = new JournalEntry(
                    new DateOnly(2026, month, day), NextRef(),
                    $"Vendor payment — {profile.Vendor}", "system", ven[profile.Vendor].Id);
                e.AddLine(acc[profile.ExpenseAccount].Id, amount, 0m); // debit expense
                e.AddLine(acc["2100"].Id, 0m, amount);                 // credit accounts payable
                e.SetRiskScore(rng.Next(2, 12));
                Post(e);
                db.JournalEntries.Add(e);
            }
        }

        // ---- Posted customer revenue (so Cash Flow has income) ----
        for (var i = 0; i < 12; i++)
        {
            var month = 1 + (i % 6);
            var day = 1 + rng.Next(0, 27);
            var amount = Math.Round(20_000m + (decimal)rng.NextDouble() * 200_000m, 2);

            var e = new JournalEntry(
                new DateOnly(2026, month, day), NextRef(),
                "Customer invoice — Acme Corp", "system", ven["Acme Corp"].Id);
            e.AddLine(acc["1200"].Id, amount, 0m); // debit accounts receivable
            e.AddLine(acc["4000"].Id, 0m, amount); // credit sales revenue
            e.SetRiskScore(rng.Next(2, 10));
            Post(e);
            db.JournalEntries.Add(e);
        }

        // ---- Pending review: amounts far above the vendor's norm (would be flagged) ----
        AddPending(db, acc, ven, NextRef(), "Hardware Supplies Ltd", "5200", 48_920m, 87);
        AddPending(db, acc, ven, NextRef(), "Globex EU Subsidiary", "5200", 126_500m, 72);
        AddPending(db, acc, ven, NextRef(), "CloudHost Services", "5200", 18_400m, 64);

        // ---- A draft and a rejected entry for grid variety ----
        var draft = new JournalEntry(new DateOnly(2026, 6, 18), NextRef(),
            "Office supplies reimbursement", "clerk", ven["Office Depot"].Id);
        draft.AddLine(acc["5200"].Id, 482.30m, 0m);
        draft.AddLine(acc["2100"].Id, 0m, 482.30m);
        db.JournalEntries.Add(draft);

        var rejected = new JournalEntry(new DateOnly(2026, 6, 17), NextRef(),
            "FX revaluation — USD receivables", "clerk", ven["Acme Corp"].Id);
        rejected.AddLine(acc["1200"].Id, 3_182.75m, 0m);
        rejected.AddLine(acc["4000"].Id, 0m, 3_182.75m);
        rejected.SetRiskScore(55);
        rejected.Submit();
        rejected.Reject("Jelena Marić", "Unsupported revaluation rate — resubmit with documentation.");
        db.JournalEntries.Add(rejected);
    }

    private static void AddPending(
        AppDbContext db, Dictionary<string, Account> acc, Dictionary<string, Vendor> ven,
        string reference, string vendor, string expenseAccount, decimal amount, int riskScore)
    {
        var e = new JournalEntry(new DateOnly(2026, 6, 20), reference,
            $"Vendor payment — {vendor}", "clerk", ven[vendor].Id);
        e.AddLine(acc[expenseAccount].Id, amount, 0m);
        e.AddLine(acc["2100"].Id, 0m, amount);
        e.SetRiskScore(riskScore);
        e.Submit(); // Draft -> PendingReview (stays for human review)
        db.JournalEntries.Add(e);
    }

    private static void Post(JournalEntry e)
    {
        e.Submit();
        e.Approve("system (auto)");
        e.Post();
    }
}
