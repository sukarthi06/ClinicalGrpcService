using ClinicalGrpcService.Infra.Exceptions;
using ClinicalGrpcService.Infra.Extensions;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ClinicalGrpcService.Infra.Services;

public class PatientRepo(
    ApplicationDbContext dbContext,
    PatientLookupHasher lookupHasher,
    PatientLookupUpdater lookupUpdater,
    ILogger<PatientRepo> logger) : IPatientRepo
{
    private static readonly ActivitySource ActivitySource = new("ClinicalGrpcService.Repository");

    // Copies the details the caller can change. Identity, audit fields and hashes are not copied.
    private static void CopyDetails(Patient from, Patient to)
    {
        to.Name = from.Name;
        to.DoB = from.DoB;
        to.Sex = from.Sex;
        to.SexByBirth = from.SexByBirth;
        to.Title = from.Title;
        to.Mrn = from.Mrn;
        to.Email = from.Email;
        to.Phone = from.Phone;
        to.AddressLine1 = from.AddressLine1;
        to.AddressLine2 = from.AddressLine2;
        to.City = from.City;
        to.State = from.State;
        to.Zip = from.Zip;
    }

    public async Task<Patient> CreateAsync(Patient entity, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(nameof(CreateAsync));

        var mrnHash = lookupHasher.HashMrn(entity.Mrn)
            ?? throw new ArgumentException("MRN is required.", nameof(entity));

        // Includes inactive patients, so a returning patient is reactivated rather than duplicated
        var existing = await dbContext.Patients
            .FirstOrDefaultAsync(p => p.OrganizationId == entity.OrganizationId
                                      && p.MrnHash == mrnHash, cancellationToken);

        if (existing is { RecordStatus: 1 })
            throw new PatientAlreadyExistsException(existing.PatientId);

        Patient saved;
        if (existing is not null)
        {
            CopyDetails(entity, existing);
            existing.RecordStatus = 1;
            existing.LastModifiedAt = DateTime.UtcNow;
            existing.LastModifiedBy = entity.CreatedBy;
            saved = existing;
        }
        else
        {
            entity.RecordStatus = 1;
            entity.CreatedAt = DateTime.UtcNow;
            dbContext.Patients.Add(entity);
            saved = entity;
        }

        await lookupUpdater.ApplyAsync(dbContext, saved, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // Another request inserted the same MRN between our check and our insert
            throw new PatientAlreadyExistsException(null);
        }

        logger.LogInformation(existing is null
                ? "Patient {PatientId} created for organization {OrganizationId}"
                : "Patient {PatientId} reactivated for organization {OrganizationId}",
            saved.PatientId.Value, saved.OrganizationId.Value);

        return saved;
    }

    public async Task<PatientBulkResult> CreateRangeAsync(
    OrganizationId organizationId,
    IReadOnlyList<Patient> patients,
    CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(nameof(CreateRangeAsync));

        if (patients.Count == 0)
            return new PatientBulkResult([]);

        // 1. Check every row and hash its MRN (messages hold only the row index, no patient data)
        var mrnHashes = new string[patients.Count];
        for (var i = 0; i < patients.Count; i++)
        {
            if (patients[i].OrganizationId != organizationId)
                throw new ArgumentException($"Patient at index {i} belongs to a different organization.", nameof(patients));

            mrnHashes[i] = lookupHasher.HashMrn(patients[i].Mrn)
                ?? throw new ArgumentException($"MRN is required (index {i}).", nameof(patients));
        }

        // 2. One query for all MRNs already in this organization, active or inactive
        var distinctHashes = mrnHashes.Distinct().ToList();
        var existingByHash = await dbContext.Patients
            .Where(p => p.OrganizationId == organizationId && distinctHashes.Contains(p.MrnHash))
            .ToDictionaryAsync(p => p.MrnHash, cancellationToken);

        // 3. Decide each row
        var items = new PatientBulkItemResult[patients.Count];
        var seenInList = new HashSet<string>();
        var toSave = new List<Patient>();

        for (var i = 0; i < patients.Count; i++)
        {
            var patient = patients[i];
            var hash = mrnHashes[i];

            // The first occurrence of an MRN wins; later repeats are rejected
            if (!seenInList.Add(hash))
            {
                items[i] = new PatientBulkItemResult(i, null, PatientBulkOutcome.DuplicateInList);
                continue;
            }

            if (existingByHash.TryGetValue(hash, out var existing))
            {
                if (existing.RecordStatus == 1)
                {
                    items[i] = new PatientBulkItemResult(i, existing.PatientId, PatientBulkOutcome.AlreadyExists);
                    continue;
                }

                CopyDetails(patient, existing);
                existing.RecordStatus = 1;
                existing.LastModifiedAt = DateTime.UtcNow;
                existing.LastModifiedBy = patient.CreatedBy;
                toSave.Add(existing);
                items[i] = new PatientBulkItemResult(i, existing.PatientId, PatientBulkOutcome.Reactivated);
            }
            else
            {
                patient.RecordStatus = 1;
                patient.CreatedAt = DateTime.UtcNow;
                dbContext.Patients.Add(patient);
                toSave.Add(patient);
                items[i] = new PatientBulkItemResult(i, patient.PatientId, PatientBulkOutcome.Created);
            }
        }

        // 4. Hashes and name tokens, then a single save
        if (toSave.Count > 0)
        {
            foreach (var p in toSave)
                await lookupUpdater.ApplyAsync(dbContext, p, cancellationToken);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation())
            {
                // Another request inserted one of these MRNs at the same time; nothing in this batch was saved
                throw new PatientAlreadyExistsException(null);
            }
        }

        var result = new PatientBulkResult(items);

        activity?.SetTag("patients.created", result.Created);
        activity?.SetTag("patients.reactivated", result.Reactivated);
        activity?.SetTag("patients.rejected", result.Rejected);

        logger.LogInformation(
            "Bulk create for organization {OrganizationId}: {Created} created, {Reactivated} reactivated, {Rejected} rejected",
            organizationId.Value, result.Created, result.Reactivated, result.Rejected);

        return result;
    }

    public async Task<bool> DeleteAsync(Patient entity, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(nameof(DeleteAsync));

        // Soft delete in one statement. It changes only status and audit fields,
        // so the hashes and name tokens stay valid (allowed under the ExecuteUpdate rule)
        var affected = await dbContext.Patients
            .Where(p => p.PatientId == entity.PatientId
                        && p.OrganizationId == entity.OrganizationId
                        && p.RecordStatus == 1)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.RecordStatus, 0)
                    .SetProperty(p => p.LastModifiedAt, DateTime.UtcNow)
                    .SetProperty(p => p.LastModifiedBy, entity.LastModifiedBy),
                cancellationToken);

        if (affected == 0)
        {
            logger.LogWarning("Patient {PatientId} not found for organization {OrganizationId} by User:{LastModifiedBy}",
                entity.PatientId.Value, entity.OrganizationId.Value, entity.LastModifiedBy);
            return false;
        }

        logger.LogInformation("Patient {PatientId} deleted for organization {OrganizationId} by User:{LastModifiedBy}",
            entity.PatientId.Value, entity.OrganizationId.Value, entity.LastModifiedBy);

        return true;
    }

    private const int MaxPageSize = 500;

    // Inherited from IRepo<T>. It has no organization, so it would return every
    // organization's patients. Blocked on purpose; use the organization overload.
    public Task<IReadOnlyList<Patient>> GetAllAsync(CancellationToken cancellationToken)
    {
        throw new NotSupportedException("Use GetAllPatientsAsync(organizationId, pageNumber, pageSize).");
    }

    public async Task<IReadOnlyList<Patient>> GetAllPatientsAsync(
        OrganizationId organizationId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(nameof(GetAllPatientsAsync));

        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, MaxPageSize);

        var patients = await dbContext.Patients
            .AsNoTracking()
            .Where(p => p.OrganizationId == organizationId && p.RecordStatus == 1)
            // Encrypted columns can't be sorted in the database, so sort by plain columns
            //.OrderBy(p => p.Name)
            //.ThenBy(p => p.PatientId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        activity?.SetTag("patients.count", patients.Count);

        return patients;
    }

    public async Task<bool> UpdateAsync(Patient entity, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(nameof(UpdateAsync));

        // Tracked (no AsNoTracking), so EF sees the changes and saves them
        var existing = await dbContext.Patients
            .FirstOrDefaultAsync(p => p.PatientId == entity.PatientId
                                      && p.OrganizationId == entity.OrganizationId
                                      && p.RecordStatus == 1, cancellationToken);

        if (existing is null)
        {
            logger.LogWarning("Patient {PatientId} not found for organization {OrganizationId} by User:{LastModifiedBy}",
                entity.PatientId.Value, entity.OrganizationId.Value, entity.LastModifiedBy);
            return false;
        }

        // Only the editable fields; identity, audit fields, status and hashes stay untouched
        CopyDetails(entity, existing);
        existing.LastModifiedAt = DateTime.UtcNow;
        existing.LastModifiedBy = entity.LastModifiedBy;

        // Recalculate the MRN/email/phone hashes; rebuild the name tokens only if the name changed
        await lookupUpdater.ApplyAsync(dbContext, existing, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // The new MRN already belongs to another patient in this organization
            throw new PatientAlreadyExistsException(null);
        }

        logger.LogInformation("Patient {PatientId} updated for organization {OrganizationId} by User:{LastModifiedBy}",
            existing.PatientId.Value, existing.OrganizationId.Value, existing.LastModifiedBy);

        return true;
    }

    // Inherited from IRepo<T>. Filters on encrypted columns compare ciphertext and never match,
    // and nothing forces an organization filter. Blocked on purpose; use SearchPatientsAsync.
    public Task<IReadOnlyList<Patient>?> SearchByAsync(Expression<Func<Patient, bool>> filter, CancellationToken cancellationToken) =>
        throw new NotSupportedException("Use SearchPatientsAsync.");

    public async Task<IReadOnlyList<Patient>> SearchPatientsAsync(
        OrganizationId organizationId,
        PatientId? patientId,
        string? mrn,
        string? email,
        string? phone,
        string? name,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(nameof(SearchPatientsAsync));

        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, MaxPageSize);

        // Same normalization as when the hashes were saved; blank input counts as "not given"
        var mrnHash = lookupHasher.HashMrn(mrn);
        var emailHash = lookupHasher.HashEmail(email);
        var phoneHash = lookupHasher.HashPhone(phone);
        var nameTokens = lookupHasher.HashNameTokens(name);
        var nameGiven = !string.IsNullOrWhiteSpace(name);

        if (mrnHash is null && emailHash is null && phoneHash is null && !nameGiven && patientId is null)
            throw new ArgumentException("At least one search field is required.");

        // A name made only of separators (e.g. "--") has no words, so it can't match anyone
        if (nameGiven && nameTokens.Count == 0)
            return [];

        // Which fields were used, never their values
        activity?.SetTag("search.mrn", mrnHash is not null);
        activity?.SetTag("search.email", emailHash is not null);
        activity?.SetTag("search.phone", phoneHash is not null);
        activity?.SetTag("search.name.words", nameTokens.Count);
        activity?.SetTag("search.patientId", patientId is not null);

        var query = dbContext.Patients
            .AsNoTracking()
            .Where(p => p.OrganizationId == organizationId && p.RecordStatus == 1);

        if (mrnHash is not null)
            query = query.Where(p => p.MrnHash == mrnHash);
        if (emailHash is not null)
            query = query.Where(p => p.EmailHash == emailHash);
        if (phoneHash is not null)
            query = query.Where(p => p.PhoneHash == phoneHash);
        if (patientId is not null)
            query = query.Where(p => p.PatientId == patientId);

        // Every word of the searched name must be one of the patient's name words
        foreach (var token in nameTokens)
            query = query.Where(p => p.NameTokens.Any(t => t.OrganizationId == organizationId
                                                           && t.TokenHash == token));

        var patients = await query
            .OrderBy(p => p.CreatedAt)
            .ThenBy(p => p.PatientId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        activity?.SetTag("patients.count", patients.Count);

        return patients;
    }

    public async Task<bool> LinkRecordingAsync(
    PatientId patientId,
    RecordingId recordingId,
    Guid? userId,
    CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity(nameof(LinkRecordingAsync));
                
        var patientExists = await dbContext.Patients.AnyAsync(
            p => p.PatientId == patientId && p.RecordStatus == 1,
            cancellationToken);
        if (!patientExists)
        {
            logger.LogWarning("Patient {PatientId} not found when linking recording {RecordingId} by User:{UserId}",
                patientId.Value, recordingId.Value, userId);
            return false;
        }

        var existing = await dbContext.PatientRecordings
            .FirstOrDefaultAsync(x => x.RecordingId == recordingId, cancellationToken);
        if (existing is not null)
        {
            if (existing.PatientId == patientId) return true;   // same link sent twice
            throw new RecordingAlreadyLinkedException();
        }

        dbContext.PatientRecordings.Add(new PatientRecording
        {
            RecordingId = recordingId,
            PatientId = patientId,
            CreatedBy = userId
        });
        var result = await dbContext.SaveChangesAsync(cancellationToken);
        if (result > 0)
        {
            logger.LogInformation("Recording {RecordingId} linked to patient {PatientId} by User:{UserId}",
                recordingId.Value, patientId.Value, userId);
        }
        return result > 0;
    }
}
