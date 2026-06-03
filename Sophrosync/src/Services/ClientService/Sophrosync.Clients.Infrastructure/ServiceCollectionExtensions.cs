using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sophrosync.Clients.Domain.Interfaces;
using Sophrosync.Clients.Infrastructure.Persistence;
using Sophrosync.Clients.Infrastructure.Repositories;

namespace Sophrosync.Clients.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var masterKey = configuration["Encryption:MasterKey"]
            ?? throw new InvalidOperationException("Encryption:MasterKey configuration value is required.");

        services.AddDbContext<ClientsDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("ClientsDb")));

        // Make the master key available to ClientsDbContext via DI. The DbContext derives a
        // per-tenant key from this at construction time (HKDF-SHA256, tenant GUID as info)
        // so each tenant's PHI is protected by a distinct AES-256-GCM key.
        services.AddSingleton(new ClientsEncryptionOptions(masterKey));

        services.AddScoped<IClientRepository, ClientRepository>();

        return services;
    }
}
