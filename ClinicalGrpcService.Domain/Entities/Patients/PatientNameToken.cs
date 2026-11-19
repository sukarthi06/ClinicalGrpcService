namespace ClinicalGrpcService.Domain.Entities.Patients;

public class PatientNameToken
{
    public OrganizationId OrganizationId { get; set; } = default!;
    public string TokenHash { get; set; } = default!;
    public PatientId PatientId { get; set; } = default!;
}