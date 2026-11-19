namespace ClinicalService.Api.Contracts;

public sealed record PatientResponse(
    Guid PatientId,
    Guid OrganizationId,
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
    string? Zip,
    DateTime CreatedAt,
    DateTime? LastModifiedAt)
{
    public override string ToString() => $"{nameof(PatientResponse)} {PatientId}";
}