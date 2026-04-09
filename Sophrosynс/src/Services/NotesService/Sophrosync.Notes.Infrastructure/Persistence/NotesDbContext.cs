using Microsoft.EntityFrameworkCore;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.Notes.Infrastructure.Persistence.Configurations;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Notes.Infrastructure.Persistence;

public sealed class NotesDbContext(
    DbContextOptions<NotesDbContext> options,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    NotesEncryptionOptions encryptionOptions) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register configuration explicitly so the correct encryption key is always passed.
        // Do NOT use ApplyConfigurationsFromAssembly — it would invoke the parameterless constructor
        // with the all-zeros placeholder key.
        modelBuilder.ApplyConfiguration(new NoteConfiguration(encryptionOptions.Key));

        // Combined query filter: exclude soft-deleted rows, enforce tenant isolation,
        // and — for therapist role — restrict to only their own notes.
        modelBuilder.Entity<Note>()
            .HasQueryFilter(e =>
                !e.IsDeleted &&
                e.TenantId == currentTenant.Id &&
                (!currentUser.IsInRole("therapist") || e.TherapistId == currentUser.Id));

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
