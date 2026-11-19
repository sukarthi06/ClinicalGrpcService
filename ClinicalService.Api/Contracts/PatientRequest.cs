namespace ClinicalService.Api.Contracts;

// Same shape for create and update; one item in a bulk create
public sealed record PatientRequest(
    string Name,
    DateOnly DoB,
    Sex? Sex,
    SexByBirth? SexByBirth,
    string? Title,
    string Mrn,
    string? Email,
    string? Phone,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? Zip)
{
    // Patient data: a record's default ToString prints every value
    public override string ToString() => nameof(PatientRequest);
}