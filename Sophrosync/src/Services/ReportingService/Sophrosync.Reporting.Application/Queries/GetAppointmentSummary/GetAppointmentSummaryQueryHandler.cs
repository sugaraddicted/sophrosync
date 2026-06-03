using MediatR;
using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.Reporting.Application.Interfaces;

namespace Sophrosync.Reporting.Application.Queries.GetAppointmentSummary;

public sealed class GetAppointmentSummaryQueryHandler(
    IScheduleServiceClient scheduleServiceClient) : IRequestHandler<GetAppointmentSummaryQuery, AppointmentSummaryDto>
{
    public async Task<AppointmentSummaryDto> Handle(GetAppointmentSummaryQuery request, CancellationToken cancellationToken)
        => await scheduleServiceClient.GetAppointmentSummaryAsync(
            request.TenantId, request.PeriodStart, request.PeriodEnd, cancellationToken);
}
