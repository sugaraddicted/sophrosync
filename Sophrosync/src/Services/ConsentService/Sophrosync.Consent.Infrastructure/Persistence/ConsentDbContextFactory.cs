using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Consent.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used exclusively by EF Core tooling (migrations, scaffolding).
/// Provides a stub <see cref="ICurrentTenant"/> so the DbContext can be instantiated
/// without a live HTTP request context.
/// Never used at runtime — runtime DI uses the registered services.
/// </summary>
public sealed class ConsentDbContextFactory : IDesignTimeDbContextFactory<ConsentDbContext>
{
    public ConsentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ConsentDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ConsentDb")
            ?? "Host=localhost;Port=5432;Database=sophrosync_consent;Username=svc_consent;Password=consent_dev_pw";

        optionsBuilder.UseNpgsql(connectionString);

        var masterKey = Environment.GetEnvironmentVariable("Encryption__MasterKey")
            ?? "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

        return new ConsentDbContext(
            optionsBuilder.Options,
            new DesignTimeTenant(),
            new ConsentEncryptionOptions(masterKey));
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
