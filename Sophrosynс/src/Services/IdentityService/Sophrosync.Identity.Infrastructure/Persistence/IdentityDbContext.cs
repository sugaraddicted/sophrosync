using Microsoft.EntityFrameworkCore;
using Sophrosync.Identity.Domain.Entities;
using Sophrosync.Identity.Domain.Interfaces;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Identity.Infrastructure.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options), IIdentityDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(b =>
        {
            b.ToTable("tenants");
            b.HasKey(t => t.Id);
            b.Property(t => t.Name).IsRequired().HasMaxLength(200);
            b.Property(t => t.TimeZone).IsRequired().HasMaxLength(100);
            b.HasQueryFilter(t => !t.IsDeleted);
        });

        modelBuilder.Entity<UserProfile>(b =>
        {
            b.ToTable("user_profiles");
            b.HasKey(p => p.Id);
            b.Property(p => p.Email).IsRequired().HasMaxLength(256);
            b.HasIndex(p => p.Email).IsUnique();
            b.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            b.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            b.Property(p => p.Role).IsRequired().HasMaxLength(50);
            b.HasQueryFilter(p => !p.IsDeleted);
        });

        base.OnModelCreating(modelBuilder);
    }

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
