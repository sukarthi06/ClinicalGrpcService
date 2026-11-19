namespace ClinicalService.Api.Validation;

public sealed class PatientRequestValidator : AbstractValidator<PatientRequest>
{
    public PatientRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Mrn).NotEmpty().MaximumLength(50);

        RuleFor(x => x.DoB)
            .NotEmpty()
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Date of birth can't be in the future.")
            .Must(d => d.Year >= 1900)
                .WithMessage("Date of birth is not valid.");

        RuleFor(x => x.Sex).IsInEnum().When(x => x.Sex is not null);
        RuleFor(x => x.SexByBirth).IsInEnum().When(x => x.SexByBirth is not null);

        RuleFor(x => x.Email)
            .EmailAddress().MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone).TenDigitPhone();

        // Matches the database column limits
        RuleFor(x => x.Title).MaximumLength(20);
        RuleFor(x => x.State).MaximumLength(50);

        RuleFor(x => x.AddressLine1).MaximumLength(200);
        RuleFor(x => x.AddressLine2).MaximumLength(200);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.Zip).MaximumLength(20);
    }
}