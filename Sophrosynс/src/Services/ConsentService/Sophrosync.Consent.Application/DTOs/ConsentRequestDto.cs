using Sophrosync.Consent.Domain.Enums;

namespace Sophrosync.Consent.Application.DTOs;

public sealed record ConsentRequestDto(
    Guid Id,
    Guid TenantId,
    Guid ClientId,
    Guid ConsentTemplateId,
    ConsentRequestStatus Status,
    DateTime ExpiresAt,
    DateTime? CompletedAt,
    DateTime CreatedAt);
