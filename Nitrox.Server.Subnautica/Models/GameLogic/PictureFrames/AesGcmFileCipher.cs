using System.Security.Cryptography;

namespace Nitrox.Server.Subnautica.Models.GameLogic.PictureFrames;

/// <summary>
/// Encrypts/decrypts picture frame bytes for on-disk storage as opaque nonce || tag || ciphertext blobs.
/// This is done without any image extension or magic header, so it can't be read by a normal image viewer
/// </summary>
internal static class AesGcmFileCipher
{
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    public static byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        byte[] ciphertext = new byte[plaintext.Length];
        byte[] tag = new byte[TagSizeBytes];

        using (AesGcm aes = new(key, TagSizeBytes))
        {
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
        }

        byte[] result = new byte[NonceSizeBytes + TagSizeBytes + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSizeBytes);
        Buffer.BlockCopy(tag, 0, result, NonceSizeBytes, TagSizeBytes);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSizeBytes + TagSizeBytes, ciphertext.Length);
        return result;
    }

    public static byte[] Decrypt(byte[] fileBytes, byte[] key)
    {
        if (fileBytes.Length < NonceSizeBytes + TagSizeBytes)
        {
            throw new CryptographicException("Picture frame file is too small to contain a nonce and tag.");
        }

        byte[] nonce = fileBytes[..NonceSizeBytes];
        byte[] tag = fileBytes[NonceSizeBytes..(NonceSizeBytes + TagSizeBytes)];
        byte[] ciphertext = fileBytes[(NonceSizeBytes + TagSizeBytes)..];
        byte[] plaintext = new byte[ciphertext.Length];

        using AesGcm aes = new(key, TagSizeBytes);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }
}
