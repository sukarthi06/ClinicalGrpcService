using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace ClinicalGrpcService.Infra.Security;

public sealed class AesGcmFieldEncryptor : IFieldEncryptor
{
    private const int KeySize = 32;     // 256-bit
    private const int NonceSize = 12;   // standard for AES-GCM
    private const int TagSize = 16;     // 128-bit authentication tag

    private readonly Dictionary<string, byte[]> _encryptionKeys;
    private readonly string _currentVersion;
    private readonly byte[] _hashKey;

    public AesGcmFieldEncryptor(IOptions<FieldEncryptionOptions> options)
    {
        var o = options.Value;

        _encryptionKeys = o.EncryptionKeys.ToDictionary(
            kv => kv.Key,
            kv => ReadKey(kv.Value, $"EncryptionKeys:{kv.Key}"));

        if (!_encryptionKeys.ContainsKey(o.CurrentKeyVersion))
            throw new InvalidOperationException(
                $"FieldEncryption:CurrentKeyVersion '{o.CurrentKeyVersion}' has no matching key in EncryptionKeys.");

        _currentVersion = o.CurrentKeyVersion;
        _hashKey = ReadKey(o.HashKey, "HashKey");

        if (_encryptionKeys.Values.Any(k => k.SequenceEqual(_hashKey)))
            throw new InvalidOperationException("FieldEncryption:HashKey must be different from the encryption keys.");
    }

    public string Encrypt(string plaintext)
    {
        var plain = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var tag = new byte[TagSize];
        var cipher = new byte[plain.Length];

        using var aes = new AesGcm(_encryptionKeys[_currentVersion], TagSize);
        aes.Encrypt(nonce, plain, cipher, tag);

        // Format: "v1:" + base64(nonce | tag | cipher)
        return $"{_currentVersion}:{Convert.ToBase64String([.. nonce, .. tag, .. cipher])}";
    }

    public string Decrypt(string ciphertext)
    {
        var separator = ciphertext.IndexOf(':');
        if (separator <= 0)
            throw new CryptographicException("Encrypted value has an invalid format.");

        var version = ciphertext[..separator];
        if (!_encryptionKeys.TryGetValue(version, out var key))
            throw new CryptographicException($"No encryption key configured for version '{version}'.");

        var data = Convert.FromBase64String(ciphertext[(separator + 1)..]);
        if (data.Length < NonceSize + TagSize)
            throw new CryptographicException("Encrypted value is too short.");

        var nonce = data.AsSpan(0, NonceSize);
        var tag = data.AsSpan(NonceSize, TagSize);
        var cipher = data.AsSpan(NonceSize + TagSize);
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain);   // throws if the value was tampered with

        return Encoding.UTF8.GetString(plain);
    }

    public string Hash(string normalizedValue)
    {
        var hash = HMACSHA256.HashData(_hashKey, Encoding.UTF8.GetBytes(normalizedValue));
        return Convert.ToHexString(hash);   // 64 characters, fits varchar(64)
    }

    private static byte[] ReadKey(string base64, string name)
    {
        byte[] key;
        try
        {
            key = Convert.FromBase64String(base64 ?? "");
        }
        catch (FormatException)
        {
            throw new InvalidOperationException($"FieldEncryption:{name} is not valid base64.");
        }

        if (key.Length != KeySize)
            throw new InvalidOperationException($"FieldEncryption:{name} must be a {KeySize}-byte key.");

        return key;
    }
}