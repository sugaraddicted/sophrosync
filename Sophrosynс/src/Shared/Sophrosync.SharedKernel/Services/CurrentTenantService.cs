using Microsoft.AspNetCore.Http;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.SharedKernel.Services;

public sealed class CurrentTenantService(IHttpContextAccessor httpContextAccessor) : ICurrentTenant
{
    public Guid Id
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user is null || user.Identity?.IsAuthenticated != true)
                return Guid.Empty; // unauthenticated (local dev / design-time)
            return user.GetTenantId();
        }
    }

    public bool IsAvailable =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
