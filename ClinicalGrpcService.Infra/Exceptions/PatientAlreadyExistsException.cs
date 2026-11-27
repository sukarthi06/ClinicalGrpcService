namespace ClinicalGrpcService.Infra.Exceptions;

// The message carries only the PatientId, never the MRN or other patient data
public sealed class PatientAlreadyExistsException(PatientId? existingPatientId)
    : ConflictException("An active patient with this MRN already exists in the organization.")
{
    public PatientId? ExistingPatientId { get; } = existingPatientId;
}