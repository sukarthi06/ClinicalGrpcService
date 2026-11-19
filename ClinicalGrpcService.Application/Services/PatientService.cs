using ClinicalGrpcService.Application.Interfaces;
using ClinicalGrpcService.Domain.Entities.Patients;
using ClinicalGrpcService.Domain.ValueObjetcs;
using ClinicalGrpcService.Infra.Interfaces;

namespace ClinicalGrpcService.Application.Services;

// No logging here: the repo already logs IDs, and everything passing through
// this class (names, MRNs, emails, phones) is patient data.
public class PatientService(IPatientRepo patientRepo) : IPatientService
{
    public Task<Patient> CreateAsync(Patient patient, CancellationToken ct) =>
        patientRepo.CreateAsync(patient, ct);

    public Task<PatientBulkResult> CreateRangeAsync(
        OrganizationId organizationId, IReadOnlyList<Patient> patients, CancellationToken ct) =>
        patientRepo.CreateRangeAsync(organizationId, patients, ct);

    public Task<bool> UpdateAsync(Patient patient, CancellationToken ct) =>
        patientRepo.UpdateAsync(patient, ct);

    // The repo's soft delete reads only these three fields
    public Task<bool> DeleteAsync(
        PatientId patientId, OrganizationId organizationId, Guid? deletedBy, CancellationToken ct) =>
        patientRepo.DeleteAsync(new Patient
        {
            PatientId = patientId,
            OrganizationId = organizationId,
            LastModifiedBy = deletedBy
        }, ct);

    public Task<IReadOnlyList<Patient>> GetAllAsync(
        OrganizationId organizationId, int pageNumber, int pageSize, CancellationToken ct) =>
        patientRepo.GetAllPatientsAsync(organizationId, pageNumber, pageSize, ct);

    // Get by ID through the search: same organization and active-patient rules
    public async Task<Patient?> GetByIdAsync(PatientId patientId, OrganizationId organizationId, CancellationToken ct)
    {
        var result = await patientRepo.SearchPatientsAsync(
            organizationId, patientId,
            mrn: null, email: null, phone: null, name: null,
            pageNumber: 1, pageSize: 1, ct);

        return result.Count == 0 ? null : result[0];
    }

    public Task<IReadOnlyList<Patient>> SearchAsync(
        OrganizationId organizationId,
        string? mrn, string? email, string? phone, string? name,
        int pageNumber, int pageSize, CancellationToken ct) =>
        patientRepo.SearchPatientsAsync(organizationId, null, mrn, email, phone, name, pageNumber, pageSize, ct);
}