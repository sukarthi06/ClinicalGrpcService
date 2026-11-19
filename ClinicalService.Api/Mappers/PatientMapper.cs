namespace ClinicalService.Api.Mappers;

// Hand-written on purpose: only the listed fields are copied, so hashes,
// tokens and status can never come from, or leak to, the client.
public static class PatientMapper
{
    public static Patient ToPatient(this PatientRequest r, OrganizationId organizationId, Guid? userId) => new()
    {
        OrganizationId = organizationId,
        Name = r.Name.Trim(),
        DoB = r.DoB,
        Sex = r.Sex,
        SexByBirth = r.SexByBirth,
        Title = Clean(r.Title),
        Mrn = r.Mrn.Trim(),
        Email = Clean(r.Email),
        Phone = Clean(r.Phone),
        AddressLine1 = Clean(r.AddressLine1),
        AddressLine2 = Clean(r.AddressLine2),
        City = Clean(r.City),
        State = Clean(r.State),
        Zip = Clean(r.Zip),
        CreatedBy = userId,
        LastModifiedBy = userId
    };

    public static PatientResponse ToResponse(this Patient p) => new(
        p.PatientId.Value, p.OrganizationId.Value,
        p.Name, p.DoB, p.Sex, p.SexByBirth, p.Title, p.Mrn,
        p.Email, p.Phone,
        p.AddressLine1, p.AddressLine2, p.City, p.State, p.Zip,
        p.CreatedAt, p.LastModifiedAt);

    public static IReadOnlyList<PatientResponse> ToResponse(this IEnumerable<Patient> patients) =>
        patients.Select(ToResponse).ToList();

    public static CreatePatientsResponse ToResponse(this PatientBulkResult result) => new(
        result.Created, result.Reactivated, result.Rejected,
        result.Items
            .Select(i => new CreatePatientsItemResponse(i.Index, i.PatientId?.Value, i.Outcome.ToString()))
            .ToList());

    // Blank optional fields become null, so nothing encrypts an empty string
    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}