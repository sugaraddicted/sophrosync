using Sophrosync.Consent.Domain.Enums;
using Sophrosync.Consent.Domain.Events;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Consent.Domain.Entities;

/// <summary>
/// A consent request issued to a specific client. Has a TTL (ExpiresAt).
/// </summary>
public sealed class ConsentRequest : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid ConsentTemplateId { get; private set; }
    public ConsentRequestStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private ConsentRequest() { }

    public static ConsentRequest Create(
        Guid tenantId,
        Guid clientId,
        Guid consentTemplateId,
        DateTime expiresAt)
    {
        if (expiresAt <= DateTime.UtcNow)
            throw new ArgumentException("ExpiresAt must be in the future.", nameof(expiresAt));

        return new ConsentRequest
        {
            TenantId = tenantId,
            ClientId = clientId,
            ConsentTemplateId = consentTemplateId,
            Status = ConsentRequestStatus.Pending,
            ExpiresAt = expiresAt
        };
    }

    public void Complete()
    {
        if (Status != ConsentRequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be completed.");
        Status = ConsentRequestStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status != ConsentRequestStatus.Pending) return;
        Status = ConsentRequestStatus.Expired;
        RaiseDomainEvent(new ConsentRequestExpiredDomainEvent(Id, TenantId, ClientId));
    }

    public void Revoke()
    {
        Status = ConsentRequestStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
    }
}
