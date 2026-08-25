using ClinicalGrpcService.Domain.Entities;
using ClinicalGrpcService.Domain.ValueObjetcs;
using ClinicalGrpcService.Infra.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ClinicalGrpcService.Infra.Services;

public class PhysicianNoteRecordingRepo(
    ApplicationDbContext dbContext,
    ILogger<PhysicianNoteRecordingRepo> logger) : IPhysicianNoteRecordingRepo
{
    private static readonly ActivitySource ActivitySource = new("ClinicalGrpcService.Repository");

    public Task<PhysicianNoteRecording?> GetByIdAsync(PhysicianNoteId id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<PhysicianNoteRecording?> GetByRecordingIdAsync(RecordingId recordingId, CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity(nameof(GetByRecordingIdAsync));

        var result = await dbContext.PhysicianNoteRecording
            .FirstOrDefaultAsync(pn => pn.RecordingId == recordingId && pn.RecordStatus == 1, ct);
        return result;
    }

    public async Task<bool> IsPhysicianNoteReadyAsync(RecordingId recordingId, CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity(nameof(IsPhysicianNoteReadyAsync));

        var result = await dbContext.PhysicianNoteRecording
            .CountAsync(pn => pn.RecordingId == recordingId && pn.RecordStatus == 1, ct);
        return (result == 1);
    }

    public async Task<bool> SaveAsync(PhysicianNoteRecording noteRecording, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(nameof(SaveAsync));

        var existingEntity = await dbContext.PhysicianNoteRecording.FindAsync(noteRecording.PhysicianNoteId, cancellationToken);
        if (existingEntity != null) 
        {
            noteRecording.LastModifiedAt = DateTime.UtcNow;
            dbContext.Update(noteRecording);
            logger.LogInformation("Physician Note Recording updated successfully for RecordingId: {RecordingId}",
                noteRecording.RecordingId);
            return true;
        }

        var res = await dbContext.PhysicianNoteRecording
            .Where(x => x.RecordingId == noteRecording.RecordingId
                &&  x.RecordStatus == 1)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.RecordStatus, 0)
                .SetProperty(x => x.LastModifiedAt, DateTime.UtcNow),
            cancellationToken);

        if (res > 0)
            logger.LogInformation("Existing physician note cleaned up for RecordingId: {RecordingId}", noteRecording.RecordingId);

        await dbContext.AddAsync(noteRecording, cancellationToken);
        var result = await dbContext.SaveChangesAsync(cancellationToken);
        if (result > 0)
            logger.LogInformation("Physician Note Recording saved successfully for RecordingId: {RecordingId}",
                noteRecording.RecordingId);
        else
            logger.LogWarning("Saving Physician Note Recording failed for RecordingId: {RecordingId}",
                noteRecording.RecordingId);

        return result > 0;
    }
}
