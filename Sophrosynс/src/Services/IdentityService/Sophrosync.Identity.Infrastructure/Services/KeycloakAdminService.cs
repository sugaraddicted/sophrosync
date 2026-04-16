using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Sophrosync.Identity.Domain.Interfaces;

namespace Sophrosync.Identity.Infrastructure.Services;

public sealed class KeycloakAdminService : IKeycloakAdminService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _baseUrl;
    private readonly string _realm;
    private readonly string _adminClientId;
    private readonly string _adminClientSecret;

    public KeycloakAdminService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _baseUrl = configuration["Keycloak:BaseUrl"]
            ?? throw new InvalidOperationException("Keycloak:BaseUrl is required.");
        _realm = configuration["Keycloak:Realm"]
            ?? throw new InvalidOperationException("Keycloak:Realm is required.");
        _adminClientId = configuration["Keycloak:AdminClientId"]
            ?? throw new InvalidOperationException("Keycloak:AdminClientId is required.");
        _adminClientSecret = configuration["Keycloak:AdminClientSecret"]
            ?? throw new InvalidOperationException("Keycloak:AdminClientSecret is required.");
    }

    public async Task<Guid> CreateUserAsync(CreateKeycloakUserRequest request, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("keycloak-admin");

        var adminToken = await GetAdminTokenAsync(client, ct);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // Create user
        var userPayload = new
        {
            username = request.Email,
            email = request.Email,
            firstName = request.FirstName,
            lastName = request.LastName,
            enabled = true,
            emailVerified = true, // TODO: set false + restore requiredActions once SMTP is configured
            attributes = new Dictionary<string, string[]>
            {
                ["tenant_id"] = [request.TenantId.ToString()]
            },
            // requiredActions = new[] { "VERIFY_EMAIL" }, // TODO: restore once SMTP is configured
            credentials = new[]
            {
                new { type = "password", value = request.Password, temporary = false }
            }
        };

        var createResponse = await client.PostAsync(
            $"/admin/realms/{_realm}/users",
            new StringContent(JsonSerializer.Serialize(userPayload), Encoding.UTF8, "application/json"),
            ct);

        if (createResponse.StatusCode == System.Net.HttpStatusCode.Conflict)
            throw new InvalidOperationException("An account with this email already exists.");

        createResponse.EnsureSuccessStatusCode();

        // Extract user ID from Location header
        var location = createResponse.Headers.Location
            ?? throw new InvalidOperationException("Keycloak did not return a user Location header.");
        var keycloakUserId = Guid.Parse(location.Segments[^1]);

        // Assign therapist realm role
        await AssignRealmRoleAsync(client, keycloakUserId, "therapist", ct);

        return keycloakUserId;
    }

    public async Task DeleteUserAsync(Guid keycloakUserId, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("keycloak-admin");
        var adminToken = await GetAdminTokenAsync(client, ct);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.DeleteAsync(
            $"/admin/realms/{_realm}/users/{keycloakUserId}", ct);

        // 404 means user already gone — treat as success
        if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
            response.EnsureSuccessStatusCode();
    }

    private async Task<string> GetAdminTokenAsync(HttpClient client, CancellationToken ct)
    {
        var tokenPayload = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _adminClientId),
            new KeyValuePair<string, string>("client_secret", _adminClientSecret),
        });

        var tokenResponse = await client.PostAsync(
            $"/realms/{_realm}/protocol/openid-connect/token", tokenPayload, ct);
        tokenResponse.EnsureSuccessStatusCode();

        var tokenJson = await tokenResponse.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(tokenJson);
        return doc.RootElement.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("Could not parse admin access token.");
    }

    private async Task AssignRealmRoleAsync(HttpClient client, Guid keycloakUserId, string roleName, CancellationToken ct)
    {
        // Get role representation
        var roleResponse = await client.GetAsync(
            $"/admin/realms/{_realm}/roles/{roleName}", ct);
        roleResponse.EnsureSuccessStatusCode();

        var roleJson = await roleResponse.Content.ReadAsStringAsync(ct);
        var roleArray = $"[{roleJson}]";

        // Assign role to user
        var assignResponse = await client.PostAsync(
            $"/admin/realms/{_realm}/users/{keycloakUserId}/role-mappings/realm",
            new StringContent(roleArray, Encoding.UTF8, "application/json"),
            ct);
        assignResponse.EnsureSuccessStatusCode();
    }
}
