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

    /// <summary>
    /// Attempts to parse the tenant_id claim from the principal.
    /// Returns false and sets tenantId to Guid.Empty when the claim is absent or malformed.
    /// Never throws.
    /// </summary>
    public static bool TryGetTenantId(this ClaimsPrincipal principal, out Guid tenantId)
    {
        var claim = principal.FindFirst(TenantIdClaimType);
        if (claim is not null && Guid.TryParse(claim.Value, out var parsed))
        {
            tenantId = parsed;
            return true;
        }

        tenantId = Guid.Empty;
        return false;
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
