using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Sophrosync.SharedKernel.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next();

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(new ValidationContext<TRequest>(request), cancellationToken)));
        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            logger.LogWarning("Validation failed for {RequestType}: {Errors}",
                typeof(TRequest).Name,
                string.Join("; ", failures.Select(f => f.ErrorMessage)));
            throw new ValidationException(failures);
        }

        return await next();
    }
}
