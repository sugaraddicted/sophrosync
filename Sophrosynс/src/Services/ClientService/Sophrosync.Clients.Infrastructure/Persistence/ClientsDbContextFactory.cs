using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Clients.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used exclusively by EF Core tooling (migrations, scaffolding).
/// Provides a stub <see cref="ICurrentTenant"/> so the DbContext can be instantiated
/// without a live HTTP request context.
/// Never used at runtime — runtime DI uses the registered <see cref="ICurrentTenant"/> service.
/// </summary>
public sealed class ClientsDbContextFactory : IDesignTimeDbContextFactory<ClientsDbContext>
{
    public ClientsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClientsDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ClientsDb")
            ?? "Host=localhost;Port=5432;Database=sophrosync_clients;Username=svc_clients;Password=clients_dev_pw";

        optionsBuilder.UseNpgsql(connectionString);

        var encKey = Environment.GetEnvironmentVariable("Encryption__ClientsKey")
            ?? "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

        return new ClientsDbContext(
            optionsBuilder.Options,
            new DesignTimeTenant(),
            new ClientsEncryptionOptions(encKey));
    }

    /// <summary>
    /// Stub tenant used only during design-time tooling execution.
    /// Returns a fixed, well-known Guid so the query filter compiles without error.
    /// </summary>
    private sealed class DesignTimeTenant : ICurrentTenant
    {
        public Guid Id { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");
        public bool HasTenant => false;
    }
}
