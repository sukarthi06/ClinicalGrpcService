namespace ClinicalService.Api.Validation;

// RecordingId.Of throws on an empty Guid, so reject it here as a 400
public sealed class LinkRecordingRequestValidator : AbstractValidator<LinkRecordingRequest>
{
    public LinkRecordingRequestValidator()
    {
        RuleFor(x => x.RecordingId).NotEmpty();
    }
}