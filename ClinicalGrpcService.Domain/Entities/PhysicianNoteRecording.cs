using ClinicalGrpcService.Domain.ValueObjetcs;

namespace ClinicalGrpcService.Domain.Entities;

public class PhysicianNoteRecording : Entity
{
    public PhysicianNoteId PhysicianNoteId { get; set; } = PhysicianNoteId.Of(Guid.NewGuid());
    public RecordingId RecordingId { get; set; } = RecordingId.Of(Guid.NewGuid());    
    public string StoragePath { get; set; } = default!;
}
