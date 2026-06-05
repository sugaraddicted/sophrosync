using FluentValidation.TestHelper;
using Sophrosync.Consent.Application.Commands.CreateConsentTemplate;
using Sophrosync.Consent.Application.Commands.IssueConsentRequest;
using Sophrosync.Consent.Application.Validators;
using Sophrosync.Consent.Domain.Enums;

namespace Sophrosync.Consent.Tests;

public sealed class ValidatorTests
{
    // -----------------------------------------------------------------------
    // CreateConsentTemplateCommandValidator
    // -----------------------------------------------------------------------

    private static CreateConsentTemplateCommand ValidCreateTemplate() =>
        new(Guid.NewGuid(), ConsentPurpose.Treatment, "GDPR Consent Form", "I consent to treatment.");

    [Fact]
    public void CreateTemplate_ValidPayload_PassesValidation()
    {
        var result = new CreateConsentTemplateCommandValidator().TestValidate(ValidCreateTemplate());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTemplate_EmptyTenantId_HasValidationError()
    {
        var result = new CreateConsentTemplateCommandValidator().TestValidate(
            ValidCreateTemplate() with { TenantId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Fact]
    public void CreateTemplate_EmptyTitle_HasValidationError()
    {
        var result = new CreateConsentTemplateCommandValidator().TestValidate(
            ValidCreateTemplate() with { Title = "" });
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreateTemplate_TitleExceedsMaxLength_HasValidationError()
    {
        var result = new CreateConsentTemplateCommandValidator().TestValidate(
            ValidCreateTemplate() with { Title = new string('x', 501) });
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreateTemplate_EmptyBodyText_HasValidationError()
    {
        var result = new CreateConsentTemplateCommandValidator().TestValidate(
            ValidCreateTemplate() with { BodyText = "" });
        result.ShouldHaveValidationErrorFor(x => x.BodyText);
    }

    // -----------------------------------------------------------------------
    // IssueConsentRequestCommandValidator
    // -----------------------------------------------------------------------

    private static IssueConsentRequestCommand ValidIssueRequest() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

    [Fact]
    public void IssueRequest_ValidPayload_PassesValidation()
    {
        var result = new IssueConsentRequestCommandValidator().TestValidate(ValidIssueRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void IssueRequest_EmptyClientId_HasValidationError()
    {
        var result = new IssueConsentRequestCommandValidator().TestValidate(
            ValidIssueRequest() with { ClientId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    [Fact]
    public void IssueRequest_EmptyTemplateId_HasValidationError()
    {
        var result = new IssueConsentRequestCommandValidator().TestValidate(
            ValidIssueRequest() with { ConsentTemplateId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ConsentTemplateId);
    }

    [Fact]
    public void IssueRequest_PastExpiresAt_HasValidationError()
    {
        var result = new IssueConsentRequestCommandValidator().TestValidate(
            ValidIssueRequest() with { ExpiresAt = DateTime.UtcNow.AddDays(-1) });
        result.ShouldHaveValidationErrorFor(x => x.ExpiresAt);
    }
}
