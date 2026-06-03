using MediatR;
using Sophrosync.Consent.Application.DTOs;
using Sophrosync.Consent.Domain.Enums;

namespace Sophrosync.Consent.Application.Queries.GetConsentStatus;

public sealed record GetConsentStatusQuery(
    Guid TenantId,
    Guid ClientId,
    ConsentPurpose Purpose) : IRequest<ConsentStatusDto>;
