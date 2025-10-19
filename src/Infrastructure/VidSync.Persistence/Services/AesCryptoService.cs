using System;
using System.Runtime.Intrinsics.Arm;
using Microsoft.Extensions.Options;
using VidSync.Domain.Interfaces;
using VidSync.Persistence.Configurations;
using System.Security.Cryptography;
using System.IO;

namespace VidSync.Persistence.Services;

public class AesCryptoService : ICryptoService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesCryptoService(IOptions<CryptoSettings> cryptoSettings)
    {
        if (cryptoSettings == null)
        {
            throw new ArgumentNullException(nameof(cryptoSettings));
        }

        _key = Convert.FromBase64String(cryptoSettings.Value.Key);
        _iv = Convert.FromBase64String(cryptoSettings.Value.IV);
    }
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            throw new ArgumentNullException(nameof(plainText));
        }

        using (var aes = System.Security.Cryptography.Aes.Create())
        {
            aes.Key = _key;
            aes.IV = _iv;

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            throw new ArgumentNullException(nameof(cipherText));
        }

        using (var aes = System.Security.Cryptography.Aes.Create())
        {
            aes.Key = _key;
            aes.IV = _iv;

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (var ms = new MemoryStream(Convert.FromBase64String(cipherText)))
            {
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}
