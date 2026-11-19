namespace ClinicalService.Api.Validation;

public sealed class GetPatientsRequestValidator : AbstractValidator<GetPatientsRequest>
{
    public GetPatientsRequestValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 500);
    }
}