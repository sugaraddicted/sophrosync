using Microsoft.EntityFrameworkCore;
using Sophrosync.SharedKernel.Abstractions;

namespace Sophrosync.SharedKernel.Security;

public static class TenantQueryExtensions
{
    /// <summary>
    /// Applies a global EF Core query filter restricting results to the current tenant.
    /// Call from OnModelCreating for every entity that has a TenantId property.
    /// </summary>
    public static void ApplyTenantFilter<TEntity>(
        this ModelBuilder modelBuilder,
        ICurrentTenant currentTenant)
        where TEntity : class
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => EF.Property<Guid>(e, "TenantId") == currentTenant.Id);
    }
}
