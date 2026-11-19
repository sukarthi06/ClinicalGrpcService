namespace ClinicalGrpcService.Infra.Security;

public sealed class FieldEncryptionOptions
{
    public const string SectionName = "FieldEncryption";

    // "v1" -> base64 key, "v2" -> base64 key, ...
    public Dictionary<string, string> EncryptionKeys { get; set; } = [];

    // Which key new data is encrypted with
    public string CurrentKeyVersion { get; set; } = "";

    // base64 key for HMAC lookups
    public string HashKey { get; set; } = "";
}