using System.Security.Cryptography;
using System.Text;
using Nitrox.Server.Subnautica.Models.GameLogic.PictureFrames;

namespace Nitrox.Test.Server.GameLogic.PictureFrames;

[TestClass]
public class AesGcmFileCipherTest
{
    private static byte[] NewKey() => RandomNumberGenerator.GetBytes(32);

    [TestMethod]
    public void EncryptThenDecrypt_ReturnsOriginalPlaintext()
    {
        byte[] key = NewKey();
        byte[] plaintext = [.. "not actually a jpeg, just some bytes to round-trip"u8];

        byte[] encrypted = AesGcmFileCipher.Encrypt(plaintext, key);
        byte[] decrypted = AesGcmFileCipher.Decrypt(encrypted, key);

        decrypted.Should().Equal(plaintext);
    }

    [TestMethod]
    public void Encrypt_DoesNotContainPlaintextBytes()
    {
        byte[] key = NewKey();
        byte[] plaintext = [.. "a very findable marker string"u8];

        byte[] encrypted = AesGcmFileCipher.Encrypt(plaintext, key);

        Encoding.UTF8.GetString(encrypted).Should().NotContain("a very findable marker string");
    }

    [TestMethod]
    public void Decrypt_TamperedCiphertext_ThrowsCryptographicException()
    {
        byte[] key = NewKey();
        byte[] plaintext = [.. "tamper test payload"u8];
        byte[] encrypted = AesGcmFileCipher.Encrypt(plaintext, key);

        encrypted[^1] ^= 0xFF;

        Invoking(() => AesGcmFileCipher.Decrypt(encrypted, key)).Should().Throw<CryptographicException>();
    }

    [TestMethod]
    public void Decrypt_WrongKey_ThrowsCryptographicException()
    {
        byte[] plaintext = [.. "wrong key test payload"u8];
        byte[] encrypted = AesGcmFileCipher.Encrypt(plaintext, NewKey());

        Invoking(() => AesGcmFileCipher.Decrypt(encrypted, NewKey())).Should().Throw<CryptographicException>();
    }
}
