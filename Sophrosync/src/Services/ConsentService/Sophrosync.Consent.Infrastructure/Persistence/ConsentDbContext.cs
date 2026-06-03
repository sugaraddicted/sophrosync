using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Sophrosync.Consent.Domain.Entities;
using Sophrosync.Consent.Infrastructure.Persistence.Configurations;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.Consent.Infrastructure.Persistence;

public sealed class ConsentDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    /// <summary>
    /// Per-tenant AES-256-GCM key derived from the master key via HKDF (SHA-256).
    /// Derived once at construction time (DbContext is scoped per request) so
    /// <see cref="OnModelCreating"/> always receives the correct tenant key.
    /// </summary>
    private readonly string _tenantEncryptionKey;

    public ConsentDbContext(
        DbContextOptions<ConsentDbContext> options,
        ICurrentTenant currentTenant,
        ConsentEncryptionOptions encryptionOptions)
        : base(options)
    {
        _currentTenant = currentTenant;

        // Derive a per-tenant key using HKDF-SHA256.
        // The tenant GUID bytes serve as the "info" parameter so each tenant receives a
        // cryptographically distinct key even though all tenants share the same master key.
        // When TenantId is Guid.Empty (design-time / migration context) the info bytes are
        // 16 zero bytes — HKDF still produces a valid, deterministic key, so migrations work.
        var ikm = Convert.FromBase64String(encryptionOptions.MasterKey);
        var info = currentTenant.Id.ToByteArray();
        var tenantKeyBytes = HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, outputLength: 32, salt: null, info: info);
        _tenantEncryptionKey = Convert.ToBase64String(tenantKeyBytes);
    }

    public DbSet<ConsentTemplate> ConsentTemplates => Set<ConsentTemplate>();
    public DbSet<ConsentRequest> ConsentRequests => Set<ConsentRequest>();
    public DbSet<ConsentRecord> ConsentRecords => Set<ConsentRecord>();
    public DbSet<ConsentDocument> ConsentDocuments => Set<ConsentDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register configurations explicitly so the correct per-tenant encryption key is always passed
        // to ConsentRecordConfiguration. Do NOT use ApplyConfigurationsFromAssembly — it would invoke
        // the parameterless constructor on ConsentRecordConfiguration, which uses the all-zeros
        // placeholder key instead of the real tenant-derived key.
        modelBuilder.ApplyConfiguration(new ConsentTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new ConsentRequestConfiguration());
        modelBuilder.ApplyConfiguration(new ConsentRecordConfiguration(_tenantEncryptionKey));
        modelBuilder.ApplyConfiguration(new ConsentDocumentConfiguration());

        modelBuilder.Entity<ConsentTemplate>()
            .HasQueryFilter(e => e.TenantId == _currentTenant.Id);
        modelBuilder.Entity<ConsentRequest>()
            .HasQueryFilter(e => e.TenantId == _currentTenant.Id);
        modelBuilder.Entity<ConsentRecord>()
            .HasQueryFilter(e => e.TenantId == _currentTenant.Id);
        modelBuilder.Entity<ConsentDocument>()
            .HasQueryFilter(d => d.TenantId == _currentTenant.Id);

        base.OnModelCreating(modelBuilder);
    }
}
