using Sophrosync.Consent.Domain.Enums;

namespace Sophrosync.Consent.Application.DTOs;

public sealed record ConsentStatusDto(
    Guid ClientId,
    ConsentPurpose Purpose,
    bool HasActiveConsent,
    DateTime? GrantedAt,
    DateTime? ExpiresAt);
