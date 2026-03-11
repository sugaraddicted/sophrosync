using Microsoft.AspNetCore.Http;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.SharedKernel.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid Id
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user is null) throw new InvalidOperationException("No HTTP context available.");
            return user.GetUserId();
        }
    }

    public string? Email =>
        httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    public IReadOnlyList<string> Roles =>
        httpContextAccessor.HttpContext?.User?.GetRoles() ?? new List<string>();

    public bool IsInRole(string role) => Roles.Contains(role);
}
