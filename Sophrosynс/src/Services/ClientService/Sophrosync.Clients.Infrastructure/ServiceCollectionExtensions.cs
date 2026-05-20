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
        var encryptionKey = configuration["Encryption:ClientsKey"]
            ?? throw new InvalidOperationException("Encryption:ClientsKey configuration value is required.");

        services.AddDbContext<ClientsDbContext>((sp, options) =>
            options.UseNpgsql(configuration.GetConnectionString("ClientsDb")));

        // Make the encryption key available to ClientsDbContext via DI so it can construct
        // ClientConfiguration(encryptionKey) explicitly rather than relying on
        // ApplyConfigurationsFromAssembly (which calls the parameterless constructor with placeholder key).
        services.AddSingleton(new ClientsEncryptionOptions(encryptionKey));

        services.AddScoped<IClientRepository, ClientRepository>();

        return services;
    }
}
