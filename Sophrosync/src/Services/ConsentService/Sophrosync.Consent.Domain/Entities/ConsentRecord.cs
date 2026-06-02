using Sophrosync.Consent.Domain.Enums;
using Sophrosync.Consent.Domain.Events;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Consent.Domain.Entities;

/// <summary>
/// Append-only consent audit record — GDPR Art. 7 proof.
/// Never updated or deleted. DB role: INSERT + SELECT only.
/// </summary>
public sealed class ConsentRecord : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid ConsentRequestId { get; private set; }
    public Guid ConsentTemplateId { get; private set; }
    public ConsentPurpose Purpose { get; private set; }
    public ConsentAction Action { get; private set; }
    public string IpAddress { get; private set; } = string.Empty; // AES-256-GCM encrypted
    public string TemplateBodySnapshot { get; private set; } = string.Empty; // AES-256-GCM encrypted
    public int TemplateVersion { get; private set; }

    private ConsentRecord() { }

    public static ConsentRecord Create(
        Guid tenantId,
        Guid clientId,
        Guid consentRequestId,
        Guid consentTemplateId,
        ConsentPurpose purpose,
        ConsentAction action,
        string ipAddress,
        string templateBodySnapshot,
        int templateVersion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(templateBodySnapshot);

        var record = new ConsentRecord
        {
            TenantId = tenantId,
            ClientId = clientId,
            ConsentRequestId = consentRequestId,
            ConsentTemplateId = consentTemplateId,
            Purpose = purpose,
            Action = action,
            IpAddress = ipAddress,
            TemplateBodySnapshot = templateBodySnapshot,
            TemplateVersion = templateVersion
        };

        record.RaiseDomainEvent(new ConsentRecordedDomainEvent(
            record.Id, tenantId, clientId, purpose, action));
        return record;
    }
}
