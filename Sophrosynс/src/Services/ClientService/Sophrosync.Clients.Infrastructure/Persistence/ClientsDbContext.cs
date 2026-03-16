using Microsoft.EntityFrameworkCore;
using Sophrosync.Clients.Domain.Entities;
using Sophrosync.Clients.Infrastructure.Persistence.Configurations;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Clients.Infrastructure.Persistence;

public sealed class ClientsDbContext(
    DbContextOptions<ClientsDbContext> options,
    ICurrentTenant currentTenant,
    ClientsEncryptionOptions encryptionOptions) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register configuration explicitly so the correct encryption key is always passed.
        // Do NOT use ApplyConfigurationsFromAssembly — it would invoke the parameterless constructor
        // with the all-zeros placeholder key.
        modelBuilder.ApplyConfiguration(new ClientConfiguration(encryptionOptions.Key));

        // Combined query filter: exclude soft-deleted rows and enforce tenant isolation.
        modelBuilder.Entity<Client>()
            .HasQueryFilter(e => !e.IsDeleted && e.TenantId == currentTenant.Id);

        base.OnModelCreating(modelBuilder);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Entity>()
            .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.TouchUpdatedAt(utcNow);
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
