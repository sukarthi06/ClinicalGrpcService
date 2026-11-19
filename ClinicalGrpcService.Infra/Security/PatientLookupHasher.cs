namespace ClinicalGrpcService.Infra.Security;

public sealed class PatientLookupHasher(IFieldEncryptor encryptor)
{
    public string? HashMrn(string? mrn) => Hash(LookupNormalizer.Mrn(mrn));
    public string? HashEmail(string? email) => Hash(LookupNormalizer.Email(email));
    public string? HashPhone(string? phone) => Hash(LookupNormalizer.Phone(phone));

    public IReadOnlyCollection<string> HashNameTokens(string? name) =>
        LookupNormalizer.NameTokens(name).Select(encryptor.Hash).ToArray();

    private string? Hash(string? normalized) =>
        normalized is null ? null : encryptor.Hash(normalized);
}