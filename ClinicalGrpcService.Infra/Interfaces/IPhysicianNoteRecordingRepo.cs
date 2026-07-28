using ClinicalGrpcService.Domain.Entities;
using ClinicalGrpcService.Domain.ValueObjetcs;

namespace ClinicalGrpcService.Infra.Interfaces;

public interface IPhysicianNoteRecordingRepo
{
    Task<bool> SaveAsync(PhysicianNoteRecording noteRecording, CancellationToken ct);
    Task<PhysicianNoteRecording?> GetByIdAsync(PhysicianNoteId id,CancellationToken ct);
    Task<PhysicianNoteRecording?> GetByRecordingIdAsync(RecordingId recordingId, CancellationToken ct);
    Task<bool> IsPhysicianNoteReadyAsync(RecordingId recordingId, CancellationToken ct);
}
