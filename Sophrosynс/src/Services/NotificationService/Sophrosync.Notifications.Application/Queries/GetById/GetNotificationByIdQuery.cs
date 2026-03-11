using MediatR;
using Sophrosync.Notifications.Application.DTOs;

namespace Sophrosync.Notifications.Application.Queries.GetById;

public sealed record GetNotificationByIdQuery(Guid NotificationId) : IRequest<NotificationDto?>;
