using ClinicalGrpcService.Domain.Entities;
using ClinicalGrpcService.Domain.ValueObjetcs;
using ClinicalGrpcService.Grpc.Protos;
using Riok.Mapperly.Abstractions;

namespace ClinicalGrpcService.Grpc.Mappers;

[Mapper]
public partial class PhysicianNoteRecordingMapper : MapperBase
{
    [MapperIgnoreTarget(nameof(PhysicianNoteRecording.CreatedAt))]
    [MapperIgnoreTarget(nameof(PhysicianNoteRecording.CreatedBy))]
    [MapperIgnoreTarget(nameof(PhysicianNoteRecording.LastModifiedAt))]
    [MapperIgnoreTarget(nameof(PhysicianNoteRecording.LastModifiedBy))]
    [MapperIgnoreTarget(nameof(PhysicianNoteRecording.RecordStatus))]
    public partial PhysicianNoteRecording ToDomain(PhysicianNoteRecordingDto dto);

    [MapperIgnoreSource(nameof(PhysicianNoteRecording.CreatedAt))]
    [MapperIgnoreSource(nameof(PhysicianNoteRecording.CreatedBy))]
    [MapperIgnoreSource(nameof(PhysicianNoteRecording.LastModifiedAt))]
    [MapperIgnoreSource(nameof(PhysicianNoteRecording.LastModifiedBy))]
    [MapperIgnoreSource(nameof(PhysicianNoteRecording.RecordStatus))]
    public partial PhysicianNoteRecordingDto ToDto(PhysicianNoteRecording entity);

    // ---- RecordingId (string <-> value object) ----
    public RecordingId MapRecordingId(string id) => RecordingId.Of(ParseGuid(id));
    private string MapRecordingId(RecordingId id) => id.Value.ToString();

    // ---- PhysicianNoteId (string <-> value object) ----
    public PhysicianNoteId MapToPhysicianNoteId(string id) => PhysicianNoteId.Of(ParseGuid(id));
    private string MapPhysicianNoteId(PhysicianNoteId id) => id.Value.ToString();
}
