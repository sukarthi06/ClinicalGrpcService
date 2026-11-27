namespace ClinicalGrpcService.Infra.Exceptions;

public sealed class RecordingAlreadyLinkedException()
    : ConflictException("The recording is already linked to another patient.");