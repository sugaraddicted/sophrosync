using Microsoft.EntityFrameworkCore;
using Sophrosync.Gateway.Data;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Gateway.Middleware;

// Spec ref: Architecture Spec Section 4.6
// On first JWT arrival for a new tenant_id, inserts a TenantProfile row into sophrosync_tenants.
// Idempotent: subsequent requests from the same tenant are no-ops.
// Security: only tenant GUID is logged — no PHI, no user PII.
public sealed class TenantProvisioningMiddleware(RequestDelegate next, ILogger<TenantProvisioningMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, TenantDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            try
            {
                var tenantId = context.User.GetTenantId();

                var exists = await db.TenantProfiles
                    .AsNoTracking()
                    .AnyAsync(t => t.Id == tenantId, context.RequestAborted);

                if (!exists)
                {
                    db.TenantProfiles.Add(new TenantProfile
                    {
                        Id = tenantId,
                        TenantName = tenantId.ToString(), // placeholder; may be enriched from JWT name claim
                        CreatedAt = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync(context.RequestAborted);
                    logger.LogInformation("Provisioned new tenant {TenantId}", tenantId);
                }
            }
            catch (InvalidOperationException ex)
            {
                // tenant_id claim missing from JWT — log and continue; Gateway JWT validation
                // will already have rejected tokens without required claims upstream.
                logger.LogWarning("TenantProvisioningMiddleware: {Message}", ex.Message);
            }
        }

        await next(context);
    }
}
