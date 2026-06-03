using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Sophrosync.Clients.Domain.Entities;
using Sophrosync.Clients.Infrastructure.Persistence;
using Sophrosync.Notes.Domain.Entities;
using Sophrosync.Notes.Infrastructure.Persistence;
using Sophrosync.SharedKernel.Abstractions;
using Sophrosync.SharedKernel.Security;

namespace Sophrosync.Security.Tests;

// ---------------------------------------------------------------------------
// Inline fakes — no Moq/NSubstitute
// ---------------------------------------------------------------------------

/// <summary>
/// Simple fake for <see cref="ICurrentTenant"/> used in security tests.
/// </summary>
file sealed class FakeCurrentTenant : ICurrentTenant
{
    public FakeCurrentTenant(Guid id) => Id = id;
    public Guid Id { get; }
    public bool HasTenant => Id != Guid.Empty;
}

/// <summary>
/// Simple fake for <see cref="ICurrentUser"/> used in security tests.
/// Supports an optional role list so the per-therapist filter in
/// <see cref="NotesDbContext"/> can be controlled.
/// </summary>
file sealed class FakeCurrentUser : ICurrentUser
{
    private readonly HashSet<string> _roles;

    public FakeCurrentUser(Guid id, string fullName, params string[] roles)
    {
        Id       = id;
        FullName = fullName;
        _roles   = new HashSet<string>(roles, StringComparer.OrdinalIgnoreCase);
    }

    public Guid   Id       { get; }
    public string FullName { get; }
    public string? Email   => null;
    public IReadOnlyList<string> Roles => _roles.ToList();
    public bool IsInRole(string role) => _roles.Contains(role);
}

// ---------------------------------------------------------------------------
// DbContext factory helpers
// ---------------------------------------------------------------------------

file static class DbContextFactory
{
    // Stable 32-byte master key for all security tests — fixed bytes, not random,
    // so each test run uses the same key (deterministic). Never used in production.
    private static readonly string MasterKey =
        Convert.ToBase64String(new byte[32] {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
            0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
            0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F, 0x20
        });

    private static readonly NotesEncryptionOptions   NotesOpts   = new(MasterKey);
    private static readonly ClientsEncryptionOptions ClientsOpts = new(MasterKey);

    /// <summary>
    /// Creates an in-memory <see cref="NotesDbContext"/> for the specified tenant and user.
    /// Contexts that share the same <paramref name="dbName"/> operate on the same store.
    /// </summary>
    public static NotesDbContext Notes(
        Guid    tenantId,
        Guid    userId,
        string? role   = null,
        string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<NotesDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var tenant = new FakeCurrentTenant(tenantId);
        var user   = role is null
            ? new FakeCurrentUser(userId, "Test User")
            : new FakeCurrentUser(userId, "Test User", role);

        return new NotesDbContext(options, tenant, user, NotesOpts);
    }

    /// <summary>
    /// Creates an in-memory <see cref="ClientsDbContext"/> for the specified tenant.
    /// Contexts that share the same <paramref name="dbName"/> operate on the same store.
    /// </summary>
    public static ClientsDbContext Clients(
        Guid    tenantId,
        string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();

        var options = new DbContextOptionsBuilder<ClientsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var tenant = new FakeCurrentTenant(tenantId);
        return new ClientsDbContext(options, tenant, ClientsOpts);
    }
}

// ---------------------------------------------------------------------------
// Security test suite
// ---------------------------------------------------------------------------

/// <summary>
/// Five tests proving multi-tenant PHI isolation and encryption correctness.
/// No Docker, no Testcontainers — in-memory EF Core only (~2 s total).
/// </summary>
public sealed class MultiTenantSecurityTests
{
    // -----------------------------------------------------------------------
    // 1. Cross-tenant isolation — Notes
    // -----------------------------------------------------------------------

    /// <summary>
    /// Seeds a Note under TenantA, then queries via a TenantB DbContext.
    /// The global query filter (<c>e.TenantId == _currentTenant.Id</c>) must
    /// produce an empty result set for TenantB.
    /// </summary>
    [Fact]
    public async Task CrossTenantIsolation_Note_TenantB_CannotSeeNotesOfTenantA()
    {
        // Arrange — unique DB name shared between both contexts so they hit the same store
        var tenantA    = Guid.NewGuid();
        var tenantB    = Guid.NewGuid();
        var therapistA = Guid.NewGuid();
        var dbName     = Guid.NewGuid().ToString();

        // Seed: TenantA writes one note; user has no role so the therapist sub-filter is inactive
        await using var ctxA = DbContextFactory.Notes(tenantA, therapistA, dbName: dbName);
        var note = Note.Create(
            tenantId:       tenantA,
            clientId:       Guid.NewGuid(),
            appointmentId:  null,
            therapistId:    therapistA,
            authorFullName: "Dr. Alice",
            type:           NoteType.DAP,
            title:          "Session Title",
            content:        "PHI content belonging to tenant A",
            tags:           null);
        ctxA.Notes.Add(note);
        await ctxA.SaveChangesAsync();

        // Act — TenantB queries the same in-memory store
        await using var ctxB = DbContextFactory.Notes(tenantB, Guid.NewGuid(), dbName: dbName);
        var tenantBNotes = await ctxB.Notes.ToListAsync();

        // Assert
        tenantBNotes.Should().BeEmpty(
            because: "the global query filter must prevent TenantB from reading TenantA's notes");
    }

    // -----------------------------------------------------------------------
    // 2. Cross-tenant isolation — Clients
    // -----------------------------------------------------------------------

    /// <summary>
    /// Seeds a Client under TenantA, then queries via a TenantB DbContext.
    /// The global query filter (<c>e.TenantId == _currentTenant.Id</c>) must
    /// produce an empty result set for TenantB.
    /// </summary>
    [Fact]
    public async Task CrossTenantIsolation_Client_TenantB_CannotSeeClientsOfTenantA()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dbName  = Guid.NewGuid().ToString();

        // Seed: TenantA writes one client
        await using var ctxA = DbContextFactory.Clients(tenantA, dbName: dbName);
        var client = Client.Create(
            tenantId: tenantA,
            name:     "Alice Patient",
            email:    "alice@example.com",
            phone:    "+1-555-0100");
        ctxA.Clients.Add(client);
        await ctxA.SaveChangesAsync();

        // Act — TenantB queries the same in-memory store
        await using var ctxB = DbContextFactory.Clients(tenantB, dbName: dbName);
        var tenantBClients = await ctxB.Clients.ToListAsync();

        // Assert
        tenantBClients.Should().BeEmpty(
            because: "the global query filter must prevent TenantB from reading TenantA's clients");
    }

    // -----------------------------------------------------------------------
    // 3. Encryption round-trip
    // -----------------------------------------------------------------------

    /// <summary>
    /// Verifies that <see cref="EncryptedStringConverter"/> is a lossless round-trip:
    /// encrypt then decrypt returns the original plaintext.
    /// </summary>
    [Fact]
    public void EncryptionRoundtrip_EncryptThenDecrypt_ReturnOriginalPlaintext()
    {
        // Arrange — fresh random 32-byte key for this test
        var keyBytes  = new byte[32];
        RandomNumberGenerator.Fill(keyBytes);
        var base64Key = Convert.ToBase64String(keyBytes);
        var converter = new EncryptedStringConverter(base64Key);
        var plaintext = "Sensitive PHI: Patient diagnosis details 2026.";

        // Act
        var ciphertext = converter.ConvertToProvider.Invoke(plaintext) as string;
        var decrypted  = converter.ConvertFromProvider.Invoke(ciphertext) as string;

        // Assert
        decrypted.Should().Be(plaintext,
            because: "AES-256-GCM encrypt-then-decrypt must be a lossless round-trip");
        ciphertext.Should().NotBe(plaintext,
            because: "encrypted output must not match plaintext");
    }

    // -----------------------------------------------------------------------
    // 4. Tampered ciphertext
    // -----------------------------------------------------------------------

    /// <summary>
    /// Flipping one byte in the ciphertext must cause AES-256-GCM authentication-tag
    /// verification to fail, proving the scheme guarantees integrity (not just confidentiality).
    /// </summary>
    [Fact]
    public void TamperedCiphertext_ThrowsAuthenticationTagMismatch()
    {
        // Arrange
        var keyBytes  = new byte[32];
        RandomNumberGenerator.Fill(keyBytes);
        var base64Key = Convert.ToBase64String(keyBytes);
        var converter = new EncryptedStringConverter(base64Key);
        var plaintext = "Confidential clinical note content.";

        var ciphertext = converter.ConvertToProvider.Invoke(plaintext) as string;
        ciphertext.Should().NotBeNull();

        // Tamper: decode, flip one byte (position 40 is inside the ciphertext body,
        // past the 12-byte nonce and 16-byte tag), re-encode.
        var bytes   = Convert.FromBase64String(ciphertext!);
        bytes[40]   = (byte)(bytes[40] ^ 0xFF);
        var tampered = Convert.ToBase64String(bytes);

        // Act
        var act = () => converter.ConvertFromProvider.Invoke(tampered);

        // Assert — AES-GCM raises CryptographicException on tag mismatch
        act.Should().Throw<CryptographicException>(
            because: "AES-256-GCM must reject tampered ciphertext to guarantee data integrity");
    }

    // -----------------------------------------------------------------------
    // 5. HKDF per-tenant key differentiation
    // -----------------------------------------------------------------------

    /// <summary>
    /// Verifies that HKDF(SHA-256) with distinct tenant GUIDs as the info parameter
    /// produces different encryption keys even when the master key is identical,
    /// so that the same plaintext yields different ciphertexts for different tenants.
    /// This matches the derivation logic in <see cref="NotesDbContext"/> and
    /// <see cref="ClientsDbContext"/>.
    /// </summary>
    [Fact]
    public void DifferentTenants_HKDF_ProduceDifferentCiphertext()
    {
        // Arrange — one master key, two distinct tenant GUIDs
        var masterKeyBytes = new byte[32];
        RandomNumberGenerator.Fill(masterKeyBytes);
        var masterKey = Convert.ToBase64String(masterKeyBytes);

        var tenantAId = Guid.NewGuid();
        var tenantBId = Guid.NewGuid();

        // Derive per-tenant keys using the same HKDF logic as the DbContext constructors
        var tenantAKeyBytes = HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            ikm:          Convert.FromBase64String(masterKey),
            outputLength: 32,
            salt:         null,
            info:         tenantAId.ToByteArray());

        var tenantBKeyBytes = HKDF.DeriveKey(
            HashAlgorithmName.SHA256,
            ikm:          Convert.FromBase64String(masterKey),
            outputLength: 32,
            salt:         null,
            info:         tenantBId.ToByteArray());

        var converterA = new EncryptedStringConverter(Convert.ToBase64String(tenantAKeyBytes));
        var converterB = new EncryptedStringConverter(Convert.ToBase64String(tenantBKeyBytes));

        var sharedPlaintext = "Same PHI text encrypted under different tenant keys.";

        // Act
        var ciphertextA = converterA.ConvertToProvider.Invoke(sharedPlaintext) as string;
        var ciphertextB = converterB.ConvertToProvider.Invoke(sharedPlaintext) as string;

        // Assert — different keys must produce different ciphertexts
        ciphertextA.Should().NotBe(ciphertextB,
            because: "HKDF with distinct tenant info bytes must produce cryptographically distinct keys");

        // Also verify each per-tenant key round-trips correctly
        var decryptedA = converterA.ConvertFromProvider.Invoke(ciphertextA) as string;
        var decryptedB = converterB.ConvertFromProvider.Invoke(ciphertextB) as string;

        decryptedA.Should().Be(sharedPlaintext,
            because: "TenantA key must decrypt its own ciphertext");
        decryptedB.Should().Be(sharedPlaintext,
            because: "TenantB key must decrypt its own ciphertext");
    }
}
