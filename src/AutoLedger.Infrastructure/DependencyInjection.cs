using AutoLedger.Domain.Abstractions;
using AutoLedger.Domain.Risk;
using AutoLedger.Domain.Services;
using AutoLedger.Infrastructure.Auditing;
using AutoLedger.Infrastructure.Persistence;
using AutoLedger.Infrastructure.Queries;
using AutoLedger.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoLedger.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Wires the data-access layer plus the domain services/strategies. Call this from the web
    /// host; the web layer may then replace <see cref="ICurrentUserAccessor"/> with an HTTP one.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();
        services.AddScoped<ICurrentUserAccessor, SystemCurrentUserAccessor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        // Data access
        services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILedgerQueries, LedgerQueries>();
        services.AddScoped<IVendorStatisticsProvider, VendorStatisticsProvider>();

        // Risk strategies (Strategy pattern) — add a new rule here and it joins the assessment.
        services.AddSingleton<IRiskStrategy, DeviationFromVendorAverageStrategy>();
        services.AddSingleton<IRiskStrategy, NewPayeeStrategy>();
        services.AddSingleton<IRiskStrategy, LargeAmountStrategy>();

        // Domain services
        services.AddScoped<RiskAssessmentService>();
        services.AddScoped<PostingService>();
        services.AddScoped<WorkflowEngine>();

        return services;
    }
}
