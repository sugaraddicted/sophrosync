using Sophrosync.Consent.Domain.Enums;

namespace Sophrosync.Consent.Application.DTOs;

public sealed record ConsentRecordDto(
    Guid Id,
    Guid TenantId,
    Guid ClientId,
    Guid ConsentRequestId,
    ConsentPurpose Purpose,
    ConsentAction Action,
    int TemplateVersion,
    DateTime CreatedAt);
