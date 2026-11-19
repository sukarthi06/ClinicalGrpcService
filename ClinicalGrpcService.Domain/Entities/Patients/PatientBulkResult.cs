namespace ClinicalGrpcService.Domain.Entities.Patients;

public enum PatientBulkOutcome
{
    Created = 1,
    Reactivated = 2,
    AlreadyExists = 3,
    DuplicateInList = 4
}

// Index = position in the input list
public sealed record PatientBulkItemResult(int Index, PatientId? PatientId, PatientBulkOutcome Outcome);

public sealed record PatientBulkResult(IReadOnlyList<PatientBulkItemResult> Items)
{
    public int Created => Items.Count(i => i.Outcome == PatientBulkOutcome.Created);
    public int Reactivated => Items.Count(i => i.Outcome == PatientBulkOutcome.Reactivated);
    public int Rejected => Items.Count(i => i.Outcome is PatientBulkOutcome.AlreadyExists or PatientBulkOutcome.DuplicateInList);
}
