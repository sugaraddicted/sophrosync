using Sophrosync.Consent.Domain.Enums;

namespace Sophrosync.Consent.Application.DTOs;

public sealed record ConsentTemplateDto(
    Guid Id,
    Guid TenantId,
    ConsentPurpose Purpose,
    string Title,
    string BodyText,
    int VersionNumber,
    ConsentTemplateStatus Status,
    DateTime? PublishedAt,
    DateTime CreatedAt);
