using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NitroxModel.Security;

/// <summary>
///     API for symmetric encryption and decryption. Uses AES with a 256-bit key. Encrypted data will get prefixed with a
///     unique AES IV for future decryption.
///     TODO: Shrink cache by removing passwords that weren't used in a while (1 minute?)
/// </summary>
/// <remarks>
///     The IV is not secret and can be shared with the encrypted data. It is used to randomize the encryption and prevent
///     the same data from encrypting to the same ciphertext.
/// </remarks>
public static class SymCrypt
{
    private const int AES_VI_SIZE = 16;
    private static readonly Dictionary<string, Aes> passwordToAesCache = new();
    private static readonly HashAlgorithm hasher = SHA256.Create();

    /// <summary>
    ///     Encrypts the byte array with the given password. The password is transformed to a 256-bit key but isn't salted.
    ///     Caller should provide a sufficiently complex password.
    /// </summary>
    public static byte[] Encrypt(string password, in byte[] data)
    {
        if (data is not { Length: > 0 })
        {
            return Array.Empty<byte>();
        }

        Aes aes = GetAes(password);
        return EncryptPlainData(in data, aes);
    }

    /// <summary>
    ///     Decrypts the byte array with the given password. The password is transformed to a 256-bit key but isn't salted.
    ///     Caller should provide a sufficiently complex password.
    /// </summary>
    /// <returns>The decrypted bytes if the password was correct. Returns an empty array otherwise.</returns>
    public static bool TryDecrypt(string password, in byte[] data, out byte[] decryptedData)
    {
        switch (data.Length)
        {
            case 0:
                decryptedData = Array.Empty<byte>();
                return true;
            case < AES_VI_SIZE:
                throw new ArgumentException($"Data is too short to be decrypted. Expected first {AES_VI_SIZE} bytes to be the AES IV.", nameof(data));
        }

        Aes aes = GetAes(password);
        decryptedData = DecryptCipherData(in data, aes);
        return decryptedData.Length > 0;
    }

    private static Aes GetAes(string password)
    {
        if (passwordToAesCache.TryGetValue(password, out Aes aes))
        {
            return aes;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password));
        }

        aes = Aes.Create();
        aes.Key = hasher.ComputeHash(Encoding.UTF8.GetBytes(password));
        aes.Padding = PaddingMode.PKCS7;

        return passwordToAesCache[password] = aes;
    }

    private static byte[] EncryptPlainData(in byte[] plainData, Aes aes)
    {
        aes.GenerateIV();

        byte[] encrypted;
        using (MemoryStream encryptorOutput = new())
        {
            // The encrypt stream needs to finalize (FlushFinalBlock, called in Dispose) before the output can be read.
            using (CryptoStream encryptorStream = new(encryptorOutput, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
            {
                encryptorStream.Write(plainData, 0, plainData.Length);
            }

            encrypted = encryptorOutput.ToArray();
        }

        // Prefix the IV (as unencrypted bytes) so that it can used as-is during decryption of payload.
        byte[] result = new byte[AES_VI_SIZE + encrypted.Length];
        Array.Copy(aes.IV, result, AES_VI_SIZE);
        Array.Copy(encrypted, 0, result, AES_VI_SIZE, encrypted.Length);

        return result;
    }

    private static byte[] DecryptCipherData(in byte[] cipherDataWithIv, Aes aes)
    {
        // Store IV into EAS instance and the remainder as input to the decryptor stream.
        byte[] dataWithoutIv = ArrayPool<byte>.Shared.Rent(cipherDataWithIv.Length - AES_VI_SIZE);
        Array.Copy(cipherDataWithIv, aes.IV, AES_VI_SIZE);
        Array.Copy(cipherDataWithIv, AES_VI_SIZE, dataWithoutIv, 0, cipherDataWithIv.Length - AES_VI_SIZE);

        MemoryStream output = new();
        try
        {
            using MemoryStream input = new(dataWithoutIv, 0, cipherDataWithIv.Length - AES_VI_SIZE);
            using CryptoStream decryptorStream = new(input, aes.CreateDecryptor(aes.Key, aes.IV), CryptoStreamMode.Read);

            int bytesRead;
            byte[] readBuffer = ArrayPool<byte>.Shared.Rent(1024);
            while ((bytesRead = decryptorStream.Read(readBuffer, 0, 1024)) > 0)
            {
                output.Write(readBuffer, 0, bytesRead);
            }
            ArrayPool<byte>.Shared.Return(readBuffer, true);

            return output.ToArray();
        }
        catch (CryptographicException)
        {
            return Array.Empty<byte>();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(dataWithoutIv, true);
            output.Dispose();
        }
    }
}
