using ClinicalGrpcService.Domain.Entities.Patients;
using ClinicalGrpcService.Domain.ValueObjetcs;

namespace ClinicalGrpcService.Application.Interfaces;

public interface IPatientService
{
    Task<Patient> CreateAsync(Patient patient, CancellationToken ct);

    Task<PatientBulkResult> CreateRangeAsync(
        OrganizationId organizationId, IReadOnlyList<Patient> patients, CancellationToken ct);

    Task<bool> UpdateAsync(Patient patient, CancellationToken ct);

    Task<bool> DeleteAsync(
        PatientId patientId, OrganizationId organizationId, Guid? deletedBy, CancellationToken ct);

    Task<IReadOnlyList<Patient>> GetAllAsync(
        OrganizationId organizationId, int pageNumber, int pageSize, CancellationToken ct);

    Task<Patient?> GetByIdAsync(PatientId patientId, OrganizationId organizationId, CancellationToken ct);

    Task<IReadOnlyList<Patient>> SearchAsync(
        OrganizationId organizationId,
        string? mrn, string? email, string? phone, string? name,
        int pageNumber, int pageSize, CancellationToken ct);
}