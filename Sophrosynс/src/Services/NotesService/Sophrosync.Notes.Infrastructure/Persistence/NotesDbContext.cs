using Microsoft.EntityFrameworkCore;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.Notes.Infrastructure.Persistence.Configurations;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Notes.Infrastructure.Persistence;

public sealed class NotesDbContext : DbContext
{
    private readonly Guid _tenantId;
    private readonly Guid _userId;
    private readonly bool _isTherapist;
    private readonly string _encryptionKey;

    public NotesDbContext(
        DbContextOptions<NotesDbContext> options,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        NotesEncryptionOptions encryptionOptions) : base(options)
    {
        _tenantId = currentTenant.Id;
        _userId = currentUser.Id;
        _isTherapist = currentUser.IsInRole("therapist");
        _encryptionKey = encryptionOptions.Key;
    }

    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register configuration explicitly so the correct encryption key is always passed.
        // Do NOT use ApplyConfigurationsFromAssembly — it would invoke the parameterless constructor
        // with the all-zeros placeholder key.
        modelBuilder.ApplyConfiguration(new NoteConfiguration(_encryptionKey));

        // Values are resolved from per-instance fields, not from captured service references,
        // so this filter remains correct under DbContext pooling.
        modelBuilder.Entity<Note>()
            .HasQueryFilter(e =>
                !e.IsDeleted &&
                e.TenantId == _tenantId &&
                (!_isTherapist || e.TherapistId == _userId));

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
