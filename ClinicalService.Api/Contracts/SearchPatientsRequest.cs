namespace ClinicalService.Api.Contracts;

public sealed record SearchPatientsRequest(
    string? Mrn,
    string? Email,
    string? Phone,
    string? Name,
    int PageNumber = 1,
    int PageSize = 50)
{
    public override string ToString() => nameof(SearchPatientsRequest);
}