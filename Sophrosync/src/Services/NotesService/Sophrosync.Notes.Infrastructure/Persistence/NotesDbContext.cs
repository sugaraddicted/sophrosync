using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.Notes.Infrastructure.Persistence.Configurations;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Notes.Infrastructure.Persistence;

public sealed class NotesDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// Per-tenant AES-256-GCM key derived from the master key via HKDF (SHA-256).
    /// Derived once at construction time (DbContext is scoped per request) so
    /// <see cref="OnModelCreating"/> always receives the correct tenant key.
    /// </summary>
    private readonly string _tenantEncryptionKey;

    public NotesDbContext(
        DbContextOptions<NotesDbContext> options,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        NotesEncryptionOptions encryptionOptions)
        : base(options)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;

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

    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register configuration explicitly so the correct per-tenant encryption key is always passed.
        // Do NOT use ApplyConfigurationsFromAssembly — it would invoke the parameterless constructor
        // with the all-zeros placeholder key.
        modelBuilder.ApplyConfiguration(new NoteConfiguration(_tenantEncryptionKey));

        // Combined query filter: exclude soft-deleted rows, enforce tenant isolation,
        // and — for therapist role — restrict to only their own notes.
        modelBuilder.Entity<Note>()
            .HasQueryFilter(e =>
                !e.IsDeleted &&
                e.TenantId == _currentTenant.Id &&
                (!_currentUser.IsInRole("therapist") || e.TherapistId == _currentUser.Id));

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
