namespace Sophrosync.Notes.Domain.Entities;

/// <summary>
/// Defines the allowed note type constants for clinical session notes.
/// </summary>
public static class NoteType
{
    public const string DAP = "DAP";
    public const string SOAP = "SOAP";
    public const string FreeForm = "FreeForm";
    public const string Intake = "Intake";
    public const string Treatment = "Treatment";
    public const string Discharge = "Discharge";

    private static readonly HashSet<string> All = [DAP, SOAP, FreeForm, Intake, Treatment, Discharge];

    public static bool IsValid(string value) => All.Contains(value);
}
