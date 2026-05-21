using MediatR;

namespace Sophrosync.Schedule.Application.Commands.MarkNoShow;

public sealed record MarkNoShowCommand(Guid Id) : IRequest<Unit>;
