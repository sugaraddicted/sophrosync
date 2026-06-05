using FluentValidation.TestHelper;
using Sophrosync.Identity.Application.Commands.RegisterPractice;
using Sophrosync.Identity.Application.Commands.UpdateProfile;

namespace Sophrosync.Identity.Tests;

public sealed class ValidatorTests
{
    // -----------------------------------------------------------------------
    // RegisterPracticeCommandValidator
    // -----------------------------------------------------------------------

    private static RegisterPracticeCommand ValidRegister() =>
        new("jane@example.com", "Password123!", "Jane", "Doe",
            "Sunflower Practice", "Europe/Kyiv", true);

    [Fact]
    public void RegisterPractice_ValidPayload_PassesValidation()
    {
        var result = new RegisterPracticeCommandValidator().TestValidate(ValidRegister());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RegisterPractice_EmptyEmail_HasValidationError()
    {
        var result = new RegisterPracticeCommandValidator().TestValidate(ValidRegister() with { Email = "" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void RegisterPractice_InvalidEmailFormat_HasValidationError()
    {
        var result = new RegisterPracticeCommandValidator().TestValidate(ValidRegister() with { Email = "not-an-email" });
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void RegisterPractice_EmptyPassword_HasValidationError()
    {
        var result = new RegisterPracticeCommandValidator().TestValidate(ValidRegister() with { Password = "" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterPractice_ShortPassword_HasValidationError()
    {
        var result = new RegisterPracticeCommandValidator().TestValidate(ValidRegister() with { Password = "abc" });
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterPractice_EmptyFirstName_HasValidationError()
    {
        var result = new RegisterPracticeCommandValidator().TestValidate(ValidRegister() with { FirstName = "" });
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void RegisterPractice_EmptyLastName_HasValidationError()
    {
        var result = new RegisterPracticeCommandValidator().TestValidate(ValidRegister() with { LastName = "" });
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void RegisterPractice_EmptyPracticeName_HasValidationError()
    {
        var result = new RegisterPracticeCommandValidator().TestValidate(ValidRegister() with { PracticeName = "" });
        result.ShouldHaveValidationErrorFor(x => x.PracticeName);
    }

    [Fact]
    public void RegisterPractice_TermsNotAccepted_HasValidationError()
    {
        var result = new RegisterPracticeCommandValidator().TestValidate(ValidRegister() with { AcceptedTerms = false });
        result.ShouldHaveValidationErrorFor(x => x.AcceptedTerms);
    }

    // -----------------------------------------------------------------------
    // UpdateProfileCommandValidator
    // -----------------------------------------------------------------------

    [Fact]
    public void UpdateProfile_ValidPayload_PassesValidation()
    {
        var result = new UpdateProfileCommandValidator().TestValidate(new UpdateProfileCommand("Jane", "Doe"));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateProfile_EmptyFirstName_HasValidationError()
    {
        var result = new UpdateProfileCommandValidator().TestValidate(new UpdateProfileCommand("", "Doe"));
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void UpdateProfile_EmptyLastName_HasValidationError()
    {
        var result = new UpdateProfileCommandValidator().TestValidate(new UpdateProfileCommand("Jane", ""));
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void UpdateProfile_FirstNameExceedsMaxLength_HasValidationError()
    {
        var result = new UpdateProfileCommandValidator().TestValidate(
            new UpdateProfileCommand(new string('x', 101), "Doe"));
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }
}
