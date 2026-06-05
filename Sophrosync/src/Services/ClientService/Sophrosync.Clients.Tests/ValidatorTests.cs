using FluentValidation.TestHelper;
using Sophrosync.Clients.Application.Commands.CreateClient;
using Sophrosync.Clients.Application.Commands.UpdateClient;
using Sophrosync.Clients.Application.Validators;
using Sophrosync.Clients.Domain.Entities;

namespace Sophrosync.Clients.Tests;

public sealed class ValidatorTests
{
    // -----------------------------------------------------------------------
    // CreateClientCommandValidator
    // -----------------------------------------------------------------------

    private static CreateClientCommand ValidCreate() =>
        new("Jane Doe", "jane@example.com", "+380501234567");

    [Fact]
    public void CreateValidator_ValidPayload_PassesValidation()
    {
        var result = new CreateClientCommandValidator().TestValidate(ValidCreate());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateValidator_EmptyName_HasValidationError()
    {
        var result = new CreateClientCommandValidator().TestValidate(ValidCreate() with { Name = "" });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateValidator_NameExceedsMaxLength_HasValidationError()
    {
        var result = new CreateClientCommandValidator().TestValidate(
            ValidCreate() with { Name = new string('x', 201) });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateValidator_EmptyEmail_HasValidationError()
    {
        var result = new CreateClientCommandValidator().TestValidate(ValidCreate() with { Email = "" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void CreateValidator_InvalidEmailFormat_HasValidationError()
    {
        var result = new CreateClientCommandValidator().TestValidate(ValidCreate() with { Email = "not-an-email" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void CreateValidator_PhoneExceedsMaxLength_HasValidationError()
    {
        var result = new CreateClientCommandValidator().TestValidate(
            ValidCreate() with { Phone = new string('1', 51) });
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    // -----------------------------------------------------------------------
    // UpdateClientCommandValidator
    // -----------------------------------------------------------------------

    private static UpdateClientCommand ValidUpdate() =>
        new(Guid.NewGuid(), "Jane Doe", "jane@example.com", "+380501234567", ClientStatus.Active);

    [Fact]
    public void UpdateValidator_ValidPayload_PassesValidation()
    {
        var result = new UpdateClientCommandValidator().TestValidate(ValidUpdate());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateValidator_EmptyId_HasValidationError()
    {
        var result = new UpdateClientCommandValidator().TestValidate(ValidUpdate() with { Id = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void UpdateValidator_EmptyName_HasValidationError()
    {
        var result = new UpdateClientCommandValidator().TestValidate(ValidUpdate() with { Name = "" });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateValidator_InvalidStatus_HasValidationError()
    {
        var result = new UpdateClientCommandValidator().TestValidate(ValidUpdate() with { Status = "discharged" });
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
