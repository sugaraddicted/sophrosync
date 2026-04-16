using Microsoft.EntityFrameworkCore;
using Sophrosync.Identity.Domain.Entities;

namespace Sophrosync.Identity.Domain.Interfaces;

public interface IIdentityDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<UserProfile> UserProfiles { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
