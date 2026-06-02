using Sophrosync.Reporting.Application.DTOs;
using Sophrosync.SharedKernel.Http;

namespace Sophrosync.Reporting.Application.Interfaces;

public interface IScheduleServiceClient : ITypedServiceClient
{
    Task<AppointmentSummaryDto> GetAppointmentSummaryAsync(Guid tenantId, DateTime from, DateTime to, CancellationToken ct = default);
}
