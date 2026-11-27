namespace ClinicalGrpcService.Domain.Entities.Patients;

// Links a recording (stored in RecordingDb) to a patient. Holds no patient data itself.
public class PatientRecording : Entity
{
    public RecordingId RecordingId { get; set; } = default!;
    public PatientId PatientId { get; set; } = default!;
}