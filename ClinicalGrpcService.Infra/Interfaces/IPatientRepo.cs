namespace ClinicalGrpcService.Infra.Interfaces;

public interface IPatientRepo : IRepo<Patient>
{
    /// <summary>
    /// Adds a list of patients for one organization. For each patient:
    /// - new MRN: created
    /// - MRN exists but inactive: reactivated and its details updated
    /// - MRN exists and active, or repeated in the same list: rejected
    /// </summary>
    Task<PatientBulkResult> CreateRangeAsync(
        OrganizationId organizationId,
        IReadOnlyList<Patient> patients,
        CancellationToken cancellationToken);

    /// <summary>
    /// Active patients of one organization, one page at a time, oldest first.
    /// </summary>
    Task<IReadOnlyList<Patient>> GetAllPatientsAsync(
        OrganizationId organizationId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Searches active patients of one organization by MRN, email, phone and/or name,
    /// using the lookup hashes. All given fields must match. Paged, oldest first.
    /// </summary>
    Task<IReadOnlyList<Patient>> SearchPatientsAsync(
        OrganizationId organizationId,
        PatientId? patientId,
        string? mrn,
        string? email,
        string? phone,
        string? name,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
}
