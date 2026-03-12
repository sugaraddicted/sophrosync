using Microsoft.EntityFrameworkCore;

namespace Sophrosync.Gateway.Data;

// Spec ref: Architecture Spec Section 4.6 + Section 6 (Database Layout)
// Database: sophrosync_tenants — the single shared tenant registry written by Gateway.
public sealed class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
{
    public DbSet<TenantProfile> TenantProfiles => Set<TenantProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantProfile>(entity =>
        {
            entity.ToTable("tenant_profiles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TenantName)
                  .HasColumnName("tenant_name")
                  .IsRequired();
            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("NOW()");
        });
    }
}
