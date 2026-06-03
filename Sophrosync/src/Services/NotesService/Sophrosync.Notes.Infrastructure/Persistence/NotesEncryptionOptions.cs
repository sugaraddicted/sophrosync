namespace Sophrosync.Notes.Infrastructure.Persistence;

/// <summary>
/// Carries the AES-256-GCM master key used to derive per-tenant keys for PHI fields
/// in the Notes database via HKDF (SHA-256).
/// Registered as a singleton so <see cref="NotesDbContext"/> can access it during
/// <c>OnModelCreating</c> without depending on <c>IConfiguration</c> directly.
/// </summary>
public sealed class NotesEncryptionOptions
{
    /// <summary>
    /// Base64-encoded 32-byte AES master key. Sourced from <c>Encryption:MasterKey</c> configuration.
    /// A unique per-tenant key is derived from this at request time using HKDF (SHA-256) with
    /// the tenant GUID as the info parameter, limiting blast radius to a single tenant if the
    /// master key is ever exposed.
    /// Never hardcode in production — inject via environment variable or secrets manager.
    /// </summary>
    public string MasterKey { get; }

    public NotesEncryptionOptions(string masterKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(masterKey);
        MasterKey = masterKey;
    }
}
