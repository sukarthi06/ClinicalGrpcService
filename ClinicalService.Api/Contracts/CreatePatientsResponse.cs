namespace ClinicalService.Api.Contracts;

// Index = position in the request list; no patient data, only IDs and outcomes
public sealed record CreatePatientsItemResponse(int Index, Guid? PatientId, string Outcome);

public sealed record CreatePatientsResponse(
    int Created,
    int Reactivated,
    int Rejected,
    IReadOnlyList<CreatePatientsItemResponse> Items);