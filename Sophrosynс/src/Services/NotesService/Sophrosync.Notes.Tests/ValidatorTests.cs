using FluentAssertions;
using FluentValidation.TestHelper;
using Sophrosync.Notes.Application.Commands.AmendNote;
using Sophrosync.Notes.Application.Commands.CreateNote;
using Sophrosync.Notes.Application.Commands.DeleteNote;
using Sophrosync.Notes.Application.Commands.LockNote;
using Sophrosync.Notes.Application.Commands.RequestCoSign;
using Sophrosync.Notes.Application.Commands.SignNote;
using Sophrosync.Notes.Application.Commands.UpdateNote;
using Sophrosync.Notes.Domain.Entities;

namespace Sophrosync.Notes.Tests;

/// <summary>
/// FluentValidation validator tests for all Notes command validators.
/// </summary>
public sealed class ValidatorTests
{
    // -----------------------------------------------------------------------
    // CreateNoteCommandValidator
    // -----------------------------------------------------------------------

    private static CreateNoteCommand ValidCreateCommand() =>
        new(
            ClientId: Guid.NewGuid(),
            AppointmentId: null,
            Type: NoteType.DAP,
            Title: "Session title",
            Content: "Session content.",
            Tags: null);

    [Fact]
    public void CreateValidator_ValidCommand_PassesValidation()
    {
        var validator = new CreateNoteCommandValidator();
        var result    = validator.TestValidate(ValidCreateCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateValidator_EmptyClientId_FailsWithError()
    {
        var validator = new CreateNoteCommandValidator();
        var cmd       = ValidCreateCommand() with { ClientId = Guid.Empty };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    [Fact]
    public void CreateValidator_EmptyContent_FailsWithError()
    {
        var validator = new CreateNoteCommandValidator();
        var cmd       = ValidCreateCommand() with { Content = "" };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreateValidator_ContentExceedsMaxLength_FailsWithError()
    {
        var validator = new CreateNoteCommandValidator();
        var cmd       = ValidCreateCommand() with { Content = new string('x', 50001) };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void CreateValidator_InvalidNoteType_FailsWithError()
    {
        var validator = new CreateNoteCommandValidator();
        var cmd       = ValidCreateCommand() with { Type = "InvalidType" };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void CreateValidator_EmptyType_FailsWithError()
    {
        var validator = new CreateNoteCommandValidator();
        var cmd       = ValidCreateCommand() with { Type = "" };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }

    [Fact]
    public void CreateValidator_TitleExceedsMaxLength_FailsWithError()
    {
        var validator = new CreateNoteCommandValidator();
        var cmd       = ValidCreateCommand() with { Title = new string('t', 201) };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreateValidator_ValidTagsString_PassesValidation()
    {
        var validator = new CreateNoteCommandValidator();
        var cmd       = ValidCreateCommand() with { Tags = "anxiety, depression" };
        var result    = validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Tags);
    }

    [Fact]
    public void CreateValidator_TagsWithInvalidCharacters_FailsWithError()
    {
        var validator = new CreateNoteCommandValidator();
        var cmd       = ValidCreateCommand() with { Tags = "tag<script>" };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Tags);
    }

    // -----------------------------------------------------------------------
    // UpdateNoteCommandValidator
    // -----------------------------------------------------------------------

    private static UpdateNoteCommand ValidUpdateCommand() =>
        new(
            Id:      Guid.NewGuid(),
            Title:   "Updated title",
            Content: "Updated content.",
            Tags:    null);

    [Fact]
    public void UpdateValidator_ValidCommand_Passes()
    {
        var validator = new UpdateNoteCommandValidator();
        var result    = validator.TestValidate(ValidUpdateCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateValidator_EmptyId_FailsWithError()
    {
        var validator = new UpdateNoteCommandValidator();
        var cmd       = ValidUpdateCommand() with { Id = Guid.Empty };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void UpdateValidator_EmptyContent_FailsWithError()
    {
        var validator = new UpdateNoteCommandValidator();
        var cmd       = ValidUpdateCommand() with { Content = "" };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void UpdateValidator_ContentExceedsMaxLength_FailsWithError()
    {
        var validator = new UpdateNoteCommandValidator();
        var cmd       = ValidUpdateCommand() with { Content = new string('x', 50001) };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void UpdateValidator_TitleExceedsMaxLength_FailsWithError()
    {
        var validator = new UpdateNoteCommandValidator();
        var cmd       = ValidUpdateCommand() with { Title = new string('t', 201) };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    // -----------------------------------------------------------------------
    // AmendNoteCommandValidator
    // -----------------------------------------------------------------------

    private static AmendNoteCommand ValidAmendCommand() =>
        new(
            Id:      Guid.NewGuid(),
            Title:   "Amended title",
            Content: "Amended content.",
            Tags:    null);

    [Fact]
    public void AmendValidator_ValidCommand_Passes()
    {
        var validator = new AmendNoteCommandValidator();
        var result    = validator.TestValidate(ValidAmendCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AmendValidator_EmptyId_FailsWithError()
    {
        var validator = new AmendNoteCommandValidator();
        var cmd       = ValidAmendCommand() with { Id = Guid.Empty };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void AmendValidator_EmptyContent_FailsWithError()
    {
        var validator = new AmendNoteCommandValidator();
        var cmd       = ValidAmendCommand() with { Content = "" };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    [Fact]
    public void AmendValidator_ContentExceedsMaxLength_FailsWithError()
    {
        var validator = new AmendNoteCommandValidator();
        var cmd       = ValidAmendCommand() with { Content = new string('x', 50001) };
        var result    = validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Content);
    }

    // -----------------------------------------------------------------------
    // LockNoteCommandValidator
    // -----------------------------------------------------------------------

    [Fact]
    public void LockValidator_ValidCommand_Passes()
    {
        var validator = new LockNoteCommandValidator();
        var result    = validator.TestValidate(new LockNoteCommand(Guid.NewGuid()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void LockValidator_EmptyId_FailsWithError()
    {
        var validator = new LockNoteCommandValidator();
        var result    = validator.TestValidate(new LockNoteCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    // -----------------------------------------------------------------------
    // SignNoteCommandValidator
    // -----------------------------------------------------------------------

    [Fact]
    public void SignValidator_ValidCommand_Passes()
    {
        var validator = new SignNoteCommandValidator();
        var result    = validator.TestValidate(new SignNoteCommand(Guid.NewGuid()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SignValidator_EmptyId_FailsWithError()
    {
        var validator = new SignNoteCommandValidator();
        var result    = validator.TestValidate(new SignNoteCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    // -----------------------------------------------------------------------
    // RequestCoSignCommandValidator
    // -----------------------------------------------------------------------

    [Fact]
    public void RequestCoSignValidator_ValidCommand_Passes()
    {
        var validator = new RequestCoSignCommandValidator();
        var result    = validator.TestValidate(new RequestCoSignCommand(Guid.NewGuid()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void RequestCoSignValidator_EmptyId_FailsWithError()
    {
        var validator = new RequestCoSignCommandValidator();
        var result    = validator.TestValidate(new RequestCoSignCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    // -----------------------------------------------------------------------
    // DeleteNoteCommandValidator
    // -----------------------------------------------------------------------

    [Fact]
    public void DeleteValidator_ValidCommand_Passes()
    {
        var validator = new DeleteNoteCommandValidator();
        var result    = validator.TestValidate(new DeleteNoteCommand(Guid.NewGuid()));
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DeleteValidator_EmptyId_FailsWithError()
    {
        var validator = new DeleteNoteCommandValidator();
        var result    = validator.TestValidate(new DeleteNoteCommand(Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
