namespace ClinicalGrpcService.Infra.Security;

public sealed class PatientLookupUpdater(PatientLookupHasher lookupHasher)
{
    public async Task ApplyAsync(DbContext db, Patient patient, CancellationToken ct)
    {
        var entry = db.Entry(patient);

        patient.MrnHash = lookupHasher.HashMrn(patient.Mrn)
            ?? throw new InvalidOperationException($"Patient {patient.PatientId.Value} has no MRN.");
        patient.EmailHash = lookupHasher.HashEmail(patient.Email);
        patient.PhoneHash = lookupHasher.HashPhone(patient.Phone);

        // Name tokens only need rebuilding when the name is new or changed
        if (entry.State == EntityState.Modified && !entry.Property(x => x.Name).IsModified)
            return;

        var tokens = entry.Collection(x => x.NameTokens);
        if (entry.State == EntityState.Modified && !tokens.IsLoaded)
            await tokens.LoadAsync(ct);

        var desired = lookupHasher.HashNameTokens(patient.Name).ToHashSet();

        patient.NameTokens.RemoveAll(t => !desired.Contains(t.TokenHash));

        foreach (var hash in desired.Except(patient.NameTokens.Select(t => t.TokenHash)).ToList())
        {
            patient.NameTokens.Add(new PatientNameToken
            {
                OrganizationId = patient.OrganizationId,
                PatientId = patient.PatientId,
                TokenHash = hash
            });
        }
    }
}