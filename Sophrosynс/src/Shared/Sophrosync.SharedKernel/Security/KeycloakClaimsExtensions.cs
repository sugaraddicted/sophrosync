using System.Security.Claims;

namespace Sophrosync.SharedKernel.Security;

public static class KeycloakClaimsExtensions
{
    private const string TenantIdClaimType = "tenant_id";

    public static Guid GetTenantId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(TenantIdClaimType)
            ?? throw new InvalidOperationException("tenant_id claim is missing from JWT.");
        return Guid.Parse(claim.Value);
    }

    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? principal.FindFirst("sub")
            ?? throw new InvalidOperationException("sub claim is missing from JWT.");
        return Guid.Parse(claim.Value);
    }

    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal principal)
    {
        return principal.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "roles")
            .Select(c => c.Value)
            .ToList();
    }
}
