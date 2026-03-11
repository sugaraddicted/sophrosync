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
            //if (user is null) throw new InvalidOperationException("No HTTP context available.");
            //return user.GetTenantId();
            return new Guid();
        }
    }

    public bool IsAvailable =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}
