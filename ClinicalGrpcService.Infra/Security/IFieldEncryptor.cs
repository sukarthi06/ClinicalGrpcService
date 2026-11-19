namespace ClinicalGrpcService.Infra.Security;

public interface IFieldEncryptor
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
    string Hash(string normalizedValue);
}