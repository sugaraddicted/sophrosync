namespace Sophrosync.Notes.Infrastructure.Persistence;

/// <summary>
/// Carries the AES-256-GCM key used to encrypt PHI fields in the Notes database.
/// Registered as a singleton so <see cref="NotesDbContext"/> can access it during
/// <c>OnModelCreating</c> without depending on <c>IConfiguration</c> directly.
/// </summary>
public sealed class NotesEncryptionOptions
{
    /// <summary>
    /// Base64-encoded 32-byte AES key. Sourced from <c>Encryption:NotesKey</c> configuration.
    /// Never hardcode in production — inject via environment variable or secrets manager.
    /// </summary>
    public string Key { get; }

    public NotesEncryptionOptions(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        Key = key;
    }
}
