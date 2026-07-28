using ClinicalGrpcService.Domain.Entities;
using ClinicalGrpcService.Domain.ValueObjetcs;

namespace ClinicalGrpcService.Application.Interfaces;

public interface IPhysicianNoteRecordingService
{
    Task<bool> SaveAsync(PhysicianNoteRecording noteRecording, CancellationToken ct);
    Task<PhysicianNoteRecording?> GetByRecordingIdAsync(RecordingId recordingId, CancellationToken ct);
    Task<bool> IsPhysicianNoteReadyAsync(RecordingId recordingId, CancellationToken ct);
}
