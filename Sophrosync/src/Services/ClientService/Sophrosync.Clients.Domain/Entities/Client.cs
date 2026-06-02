using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Clients.Domain.Entities;

/// <summary>
/// Aggregate root representing a therapy client within a tenant.
/// Name, Email, and Phone are PHI — encrypted at the Infrastructure layer via EncryptedStringConverter.
/// </summary>
public sealed class Client : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Status { get; private set; } = ClientStatus.Active;

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private Client() { }

    /// <summary>
    /// Creates a new Client aggregate.
    /// </summary>
    public static Client Create(
        Guid tenantId,
        string name,
        string email,
        string phone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        return new Client
        {
            TenantId = tenantId,
            Name = name,
            Email = email,
            Phone = phone ?? string.Empty,
            Status = ClientStatus.Active
        };
    }

    /// <summary>
    /// Updates mutable client fields.
    /// </summary>
    public void Update(string name, string email, string phone, string status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (status != ClientStatus.Active && status != ClientStatus.Inactive)
            throw new ArgumentException($"Invalid client status: '{status}'.", nameof(status));

        Name = name;
        Email = email;
        Phone = phone ?? string.Empty;
        Status = status;
    }

    /// <summary>
    /// Marks the client as deleted without removing the row from the database.
    /// The global query filter in <c>ClientsDbContext</c> ensures deleted clients
    /// are invisible to all subsequent queries.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}
