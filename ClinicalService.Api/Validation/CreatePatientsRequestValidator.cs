namespace ClinicalService.Api.Validation;

public sealed class CreatePatientsRequestValidator : AbstractValidator<CreatePatientsRequest>
{
    private const int MaxPatients = 1000;

    public CreatePatientsRequestValidator()
    {
        RuleFor(x => x.Patients)
            .NotEmpty()
            .Must(p => p is null || p.Count <= MaxPatients)
                .WithMessage($"At most {MaxPatients} patients per request.");

        // Errors come back as "Patients[3].Phone", by row index, never by value
        RuleForEach(x => x.Patients).SetValidator(new PatientRequestValidator());
    }
}