using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace Sophrosync.SharedKernel.Security;

/// <summary>
/// Flattens Keycloak's <c>realm_access.roles</c> JSON object into individual
/// <c>roles</c> claims so that ASP.NET Core policy checks like
/// <c>RequireClaim("roles", "therapist")</c> work without requiring a custom
/// Keycloak token mapper.
///
/// Keycloak emits realm roles as a nested object:
/// <code>{ "realm_access": { "roles": ["therapist", "offline_access"] } }</code>
/// ASP.NET Core's JWT handler stores this as a single claim whose value is the
/// raw JSON string, not as individual role claims. This transformation extracts
/// them and adds a <c>roles</c> claim per entry on first evaluation.
/// </summary>
public sealed class KeycloakRolesTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Already flattened (e.g. via a Keycloak token mapper) — nothing to do.
        if (principal.HasClaim(c => c.Type == "roles"))
            return Task.FromResult(principal);

        var realmAccessClaim = principal.FindFirst("realm_access");
        if (realmAccessClaim is null)
            return Task.FromResult(principal);

        try
        {
            using var doc = JsonDocument.Parse(realmAccessClaim.Value);
            if (!doc.RootElement.TryGetProperty("roles", out var rolesElement))
                return Task.FromResult(principal);

            var identity = new ClaimsIdentity();
            foreach (var role in rolesElement.EnumerateArray())
            {
                var value = role.GetString();
                if (!string.IsNullOrEmpty(value))
                    identity.AddClaim(new Claim("roles", value));
            }

            principal.AddIdentity(identity);
        }
        catch (JsonException)
        {
            // realm_access claim is malformed JSON — skip transformation safely.
        }

        return Task.FromResult(principal);
    }
}
