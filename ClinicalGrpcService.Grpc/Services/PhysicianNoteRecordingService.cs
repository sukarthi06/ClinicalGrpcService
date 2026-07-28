using ClinicalGrpcService.Application.Interfaces;
using ClinicalGrpcService.Grpc.Mappers;
using ClinicalGrpcService.Grpc.Protos;
using Grpc.Core;

namespace ClinicalGrpcService.Grpc.Services;

public class PhysicianNoteRecordingService(
    IPhysicianNoteRecordingService service,
    PhysicianNoteRecordingMapper mapper,
    ILogger<PhysicianNoteRecordingService> logger) : PhysiciansNoteRecording.PhysiciansNoteRecordingBase
{
    public override async Task<PhysicianNoteRecordingResponse> Save(PhysicianNoteRecordingRequest request, ServerCallContext context)
    {
        logger.LogInformation("Saving Physician Note Recording...");
        var response = await service.SaveAsync(mapper.ToDomain(request.PhysicianNoteEcording), context.CancellationToken);
        return new PhysicianNoteRecordingResponse { IsSuccess = response };
    }
    public override async Task<GetByRecordingIdResponse> GetByRecordingId(GetByRecordingIdRequest request, ServerCallContext context)
    {
        var response = await service.GetByRecordingIdAsync(mapper.MapRecordingId(request.RecordingId), context.CancellationToken);

        return response is null
                ? new GetByRecordingIdResponse()
                : new GetByRecordingIdResponse { PhysicianNoteEcording = mapper.ToDto(response) };
    }
    public override async Task<IsReadyResponse> IsReady(IsReadyRequest request, ServerCallContext context)
    {
        var resposne = await service.IsPhysicianNoteReadyAsync(mapper.MapRecordingId(request.RecordingId),ct:context.CancellationToken);
        return new IsReadyResponse { IsSuccess = resposne };
    }
}
