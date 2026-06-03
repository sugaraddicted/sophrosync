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

    public string FullName
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user is null) return string.Empty;

            // Try Keycloak's "name" claim first (full display name)
            var name = user.FindFirst("name")?.Value;
            if (!string.IsNullOrWhiteSpace(name)) return name;

            // Fallback: concatenate given_name + family_name
            var given = user.FindFirst("given_name")?.Value;
            var family = user.FindFirst("family_name")?.Value;
            if (!string.IsNullOrWhiteSpace(given) || !string.IsNullOrWhiteSpace(family))
                return $"{given} {family}".Trim();

            // Final fallbacks
            return Email ?? Id.ToString();
        }
    }

    public IReadOnlyList<string> Roles =>
        httpContextAccessor.HttpContext?.User?.GetRoles() ?? new List<string>();

    public bool IsInRole(string role) => Roles.Contains(role);
}
