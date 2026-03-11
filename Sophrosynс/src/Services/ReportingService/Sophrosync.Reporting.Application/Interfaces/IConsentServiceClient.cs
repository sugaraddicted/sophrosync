using Sophrosync.SharedKernel.Http;

namespace Sophrosync.Reporting.Application.Interfaces;

public interface IConsentServiceClient : ITypedServiceClient
{
    Task<bool> GetConsentStatusAsync(Guid tenantId, Guid clientId, string purpose, CancellationToken ct = default);
}
