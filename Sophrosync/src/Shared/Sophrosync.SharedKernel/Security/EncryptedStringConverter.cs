using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Sophrosync.SharedKernel.Security;

/// <summary>
/// EF Core value converter that encrypts/decrypts string columns using AES-256-GCM.
/// Key is a 32-byte base64 value from configuration.
/// </summary>
public sealed class EncryptedStringConverter : ValueConverter<string?, string?>
{
    public EncryptedStringConverter(string base64Key)
        : base(
            v => Encrypt(v, Convert.FromBase64String(base64Key)),
            v => Decrypt(v, Convert.FromBase64String(base64Key)))
    {
    }

    private static string? Encrypt(string? plainText, byte[] key)
    {
        if (plainText is null) return null;
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];
        using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
        var result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length + tag.Length, cipherBytes.Length);
        return Convert.ToBase64String(result);
    }

    private static string? Decrypt(string? cipherText, byte[] key)
    {
        if (cipherText is null) return null;
        var allBytes = Convert.FromBase64String(cipherText);
        var nonceSize = AesGcm.NonceByteSizes.MaxSize;
        var tagSize = AesGcm.TagByteSizes.MaxSize;
        var nonce = allBytes[..nonceSize];
        var tag = allBytes[nonceSize..(nonceSize + tagSize)];
        var cipherBytes = allBytes[(nonceSize + tagSize)..];
        var plainBytes = new byte[cipherBytes.Length];
        using var aes = new AesGcm(key, tagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
