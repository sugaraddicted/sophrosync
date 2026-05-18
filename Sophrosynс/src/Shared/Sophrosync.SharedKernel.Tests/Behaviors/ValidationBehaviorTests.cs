using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Sophrosync.SharedKernel.Behaviors;
using Xunit;

namespace Sophrosync.SharedKernel.Tests.Behaviors;

public sealed class ValidationBehaviorTests
{
    private sealed record TestRequest : IRequest<string>;

    private sealed class PassingValidator : AbstractValidator<TestRequest>;

    private sealed class FailingValidator : AbstractValidator<TestRequest>
    {
        public FailingValidator(params string[] messages)
        {
            foreach (var msg in messages)
                RuleFor(_ => _).Must(_ => false).WithMessage(msg);
        }
    }

    private sealed class AsyncValidator : AbstractValidator<TestRequest>
    {
        public bool Ran { get; private set; }

        public AsyncValidator()
        {
            RuleFor(_ => _).MustAsync(async (_, ct) =>
            {
                await Task.Delay(5, ct);
                Ran = true;
                return true;
            });
        }
    }

    private static RequestHandlerDelegate<string> NextReturning(string value) =>
        () => Task.FromResult(value);

    private static ValidationBehavior<TestRequest, string> Build(
        IEnumerable<IValidator<TestRequest>> validators) =>
        new(validators, NullLogger<ValidationBehavior<TestRequest, string>>.Instance);

    [Fact]
    public async Task NoValidators_CallsNext_ReturnsResult()
    {
        var behavior = Build([]);

        var result = await behavior.Handle(new TestRequest(), NextReturning("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task SinglePassingValidator_CallsNext_ReturnsResult()
    {
        var behavior = Build([new PassingValidator()]);

        var result = await behavior.Handle(new TestRequest(), NextReturning("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task SingleFailingValidator_ThrowsValidationException_NextNotCalled()
    {
        var nextCalled = false;
        var behavior = Build([new FailingValidator("Field is required.")]);

        var act = async () => await behavior.Handle(
            new TestRequest(),
            () => { nextCalled = true; return Task.FromResult("ok"); },
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Field is required.*");

        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task MultipleFailingValidators_AllErrorsCollected()
    {
        var behavior = Build([
            new FailingValidator("Error A"),
            new FailingValidator("Error B"),
        ]);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new TestRequest(), NextReturning("ok"), CancellationToken.None));

        ex.Errors.Should().HaveCount(2);
        ex.Errors.Select(e => e.ErrorMessage).Should().Contain(["Error A", "Error B"]);
    }

    [Fact]
    public async Task AsyncValidator_IsAwaited_RunsBeforeNext()
    {
        var validator = new AsyncValidator();
        var behavior = Build([validator]);

        await behavior.Handle(new TestRequest(), NextReturning("ok"), CancellationToken.None);

        validator.Ran.Should().BeTrue();
    }

    [Fact]
    public async Task MixedValidators_OnePassingOneFailing_ThrowsWithOnlyFailureErrors()
    {
        var behavior = Build([
            new PassingValidator(),
            new FailingValidator("Must not be empty"),
        ]);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new TestRequest(), NextReturning("ok"), CancellationToken.None));

        ex.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Must not be empty");
    }
}
