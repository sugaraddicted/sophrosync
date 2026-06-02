namespace Sophrosync.Clients.Infrastructure.Persistence;

/// <summary>
/// Carries the AES-256-GCM key used to encrypt PHI fields in the Clients database.
/// Registered as a singleton so <see cref="ClientsDbContext"/> can access it during
/// <c>OnModelCreating</c> without depending on <c>IConfiguration</c> directly.
/// </summary>
public sealed class ClientsEncryptionOptions
{
    /// <summary>
    /// Base64-encoded 32-byte AES key. Sourced from <c>Encryption:ClientsKey</c> configuration.
    /// Never hardcode in production — inject via environment variable or secrets manager.
    /// </summary>
    public string Key { get; }

    public ClientsEncryptionOptions(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        Key = key;
    }
}
