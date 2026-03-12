namespace Sophrosync.Gateway.Data;

// Spec ref: Architecture Spec Section 4.6 — Tenant provisioning middleware
// Shared table read by all services. Gateway inserts on first authenticated JWT arrival.
// Database: sophrosync_tenants / table: tenant_profiles
public sealed class TenantProfile
{
    public Guid Id { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
