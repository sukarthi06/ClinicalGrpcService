using ClinicalGrpcService.Application.Interfaces;
using ClinicalGrpcService.Domain.Entities;
using ClinicalGrpcService.Domain.ValueObjetcs;
using ClinicalGrpcService.Infra.Interfaces;
using Microsoft.Extensions.Logging;

namespace ClinicalGrpcService.Application.Services;

public class PhysicianNoteRecordingService(
    IPhysicianNoteRecordingRepo noteRecordingRepo,
    ILogger<PhysicianNoteRecordingService> logger) : IPhysicianNoteRecordingService
{
    public Task<PhysicianNoteRecording?> GetByRecordingIdAsync(RecordingId recordingId, CancellationToken ct)
    {
        return noteRecordingRepo.GetByRecordingIdAsync(recordingId, ct);
    }

    public async Task<bool> IsPhysicianNoteReadyAsync(RecordingId recordingId, CancellationToken ct)
    {
        return await noteRecordingRepo.IsPhysicianNoteReadyAsync(recordingId, ct);
    }

    public Task<bool> SaveAsync(PhysicianNoteRecording noteRecording, CancellationToken ct)
    {
        logger.LogInformation("Saving Physician Note Recording for RecordingId: {RecordingId}", noteRecording.RecordingId);
        return noteRecordingRepo.SaveAsync(noteRecording, ct);
    }
}
