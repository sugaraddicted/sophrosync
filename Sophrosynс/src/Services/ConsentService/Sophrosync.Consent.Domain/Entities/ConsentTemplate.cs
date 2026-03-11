using Sophrosync.Consent.Domain.Enums;
using Sophrosync.Consent.Domain.Events;
using Sophrosync.SharedKernel.Domain;

namespace Sophrosync.Consent.Domain.Entities;

/// <summary>
/// Versioned consent template. Immutable once Published (GDPR Art. 7 proof requirement).
/// </summary>
public sealed class ConsentTemplate : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public ConsentPurpose Purpose { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string BodyText { get; private set; } = string.Empty;
    public int VersionNumber { get; private set; }
    public ConsentTemplateStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public DateTime? RetiredAt { get; private set; }

    private ConsentTemplate() { }

    public static ConsentTemplate Create(
        Guid tenantId,
        ConsentPurpose purpose,
        string title,
        string bodyText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(bodyText);

        return new ConsentTemplate
        {
            TenantId = tenantId,
            Purpose = purpose,
            Title = title,
            BodyText = bodyText,
            VersionNumber = 1,
            Status = ConsentTemplateStatus.Draft
        };
    }

    public void Publish()
    {
        if (Status != ConsentTemplateStatus.Draft)
            throw new InvalidOperationException("Only draft templates can be published.");
        Status = ConsentTemplateStatus.Published;
        PublishedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ConsentTemplatePublishedDomainEvent(Id, TenantId, Purpose, VersionNumber));
    }

    public void Retire()
    {
        if (Status != ConsentTemplateStatus.Published)
            throw new InvalidOperationException("Only published templates can be retired.");
        Status = ConsentTemplateStatus.Retired;
        RetiredAt = DateTime.UtcNow;
    }
}
