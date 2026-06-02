using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;

namespace Sophrosync.SharedKernel.Http;

/// <summary>
/// Factory for standard Polly retry + circuit-breaker policies used by all typed HTTP clients.
/// </summary>
public static class ResiliencePolicy
{
    /// <summary>
    /// 3 retries with exponential back-off (1s, 2s, 4s) + circuit breaker (5 failures → 30s open).
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryWithCircuitBreaker()
    {
        var retry = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)),
                onRetry: (outcome, timespan, attempt, _) =>
                {
                    // Logging is done by the typed client or middleware
                });

        var circuitBreaker = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30));

        return Policy.WrapAsync(retry, circuitBreaker);
    }

    /// <summary>
    /// Convenience method to register a typed HTTP client with the standard resilience policy.
    /// </summary>
    public static IHttpClientBuilder AddResilientHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string baseAddressConfigKey)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services
            .AddHttpClient<TClient, TImplementation>()
            .AddPolicyHandler(GetRetryWithCircuitBreaker());
    }
}
