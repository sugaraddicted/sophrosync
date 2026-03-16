using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.SharedKernel.Services;

/// <summary>
/// Resolves the current tenant from the active JWT via IHttpContextAccessor.
/// Per spec Section 3.4: tenant isolation enforced at the HTTP context layer.
/// PHI compliance: tenant GUID logged at Debug level only — no PII written to logs.
/// </summary>
public sealed class CurrentTenantService(
    IHttpContextAccessor httpContextAccessor,
    ILogger<CurrentTenantService> logger) : ICurrentTenant
{
    public Guid Id
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal is null)
            {
                logger.LogDebug("CurrentTenantService: no HTTP context available, returning Guid.Empty.");
                return Guid.Empty;
            }

            if (principal.TryGetTenantId(out var tenantId))
            {
                logger.LogDebug("CurrentTenantService: resolved tenant {TenantId}.", tenantId);
                return tenantId;
            }

            logger.LogDebug("CurrentTenantService: tenant_id claim absent or malformed, returning Guid.Empty.");
            return Guid.Empty;
        }
    }

    public bool HasTenant =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true
        && httpContextAccessor.HttpContext.User.TryGetTenantId(out _);
}
