using Microsoft.AspNetCore.Http;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.SharedKernel.Services;

// Spec ref: Architecture Spec Section 5 (SharedKernel) + Section 3.4 (Security Patterns)
// Resolves tenant_id from the "tenant_id" custom JWT claim injected by Keycloak Protocol Mapper.
// Returns Guid.Empty when the user is not authenticated — callers must check IsAvailable first.
public sealed class CurrentTenantService(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    public Guid Id
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user is null || user.Identity?.IsAuthenticated != true)
                return Guid.Empty;
            return user.GetTenantId();
        }
    }

    public bool IsAvailable =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
