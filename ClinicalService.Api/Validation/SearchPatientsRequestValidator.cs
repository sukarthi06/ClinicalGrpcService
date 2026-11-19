namespace ClinicalService.Api.Validation;

public sealed class SearchPatientsRequestValidator : AbstractValidator<SearchPatientsRequest>
{
    public SearchPatientsRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Mrn)
                       || !string.IsNullOrWhiteSpace(x.Email)
                       || !string.IsNullOrWhiteSpace(x.Phone)
                       || !string.IsNullOrWhiteSpace(x.Name))
            .WithName("Search")
            .WithMessage("At least one of MRN, email, phone or name is required.");

        RuleFor(x => x.Phone).TenDigitPhone();

        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
    }
}