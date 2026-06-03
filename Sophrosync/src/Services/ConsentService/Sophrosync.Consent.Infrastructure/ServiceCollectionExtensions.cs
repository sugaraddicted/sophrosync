using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sophrosync.Consent.Domain.Interfaces;
using Sophrosync.Consent.Infrastructure.Persistence;
using Sophrosync.Consent.Infrastructure.Repositories;

namespace Sophrosync.Consent.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var masterKey = configuration["Encryption:MasterKey"]
            ?? throw new InvalidOperationException("Encryption:MasterKey configuration value is required.");

        services.AddDbContext<ConsentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ConsentDb")));

        // Make the master key available to ConsentDbContext via DI. The DbContext derives a
        // per-tenant key from this at construction time (HKDF-SHA256, tenant GUID as info)
        // so each tenant's PHI is protected by a distinct AES-256-GCM key.
        services.AddSingleton(new ConsentEncryptionOptions(masterKey));

        services.AddMemoryCache();

        services.AddScoped<IConsentTemplateRepository, ConsentTemplateRepository>();
        services.AddScoped<IConsentRequestRepository, ConsentRequestRepository>();
        services.AddScoped<IConsentRecordRepository, ConsentRecordRepository>();
        services.AddScoped<IConsentDocumentRepository, ConsentDocumentRepository>();

        return services;
    }
}
