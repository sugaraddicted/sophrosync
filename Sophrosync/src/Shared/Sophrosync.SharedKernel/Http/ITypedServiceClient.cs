namespace Sophrosync.SharedKernel.Http;

/// <summary>
/// Marker interface for all typed HTTP clients that communicate between services.
/// All implementors should be wrapped with Polly retry + circuit breaker via ResiliencePolicy.
/// </summary>
public interface ITypedServiceClient
{
    string ServiceName { get; }
}
