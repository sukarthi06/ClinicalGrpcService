namespace ClinicalService.Api.Contracts;

public sealed record CreatePatientsRequest(IReadOnlyList<PatientRequest> Patients)
{
    public override string ToString() => $"{nameof(CreatePatientsRequest)} ({Patients?.Count ?? 0} patients)";
}