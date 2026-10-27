using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;

namespace StudentCabinetMVVM.Services;

public class AesEncryptionService : IEncryptionService
{
    private const string KeyName = "db_encryption_key";
    private byte[]? _key;

    public async Task<string> EncryptAsync(string plainText)
    {
        var key = await GetKeyAsync();

        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText ?? string.Empty);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public async Task<string> DecryptAsync(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        var key = await GetKeyAsync();
        var data = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = key;

        var iv = new byte[aes.BlockSize / 8];
        Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(data, iv.Length, data.Length - iv.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private async Task<byte[]> GetKeyAsync()
    {
        if (_key is not null)
            return _key;

        var stored = await SecureStorage.Default.GetAsync(KeyName);
        if (stored is null)
        {
            stored = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            await SecureStorage.Default.SetAsync(KeyName, stored);
        }

        _key = Convert.FromBase64String(stored);
        return _key;
    }
}