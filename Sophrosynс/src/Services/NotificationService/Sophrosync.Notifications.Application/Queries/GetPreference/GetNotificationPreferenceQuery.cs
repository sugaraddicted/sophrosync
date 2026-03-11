using MediatR;
using Sophrosync.Notifications.Application.DTOs;

namespace Sophrosync.Notifications.Application.Queries.GetPreference;

public sealed record GetNotificationPreferenceQuery(Guid TenantId, Guid UserId) : IRequest<NotificationPreferenceDto?>;
