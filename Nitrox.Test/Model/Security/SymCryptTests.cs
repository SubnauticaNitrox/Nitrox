using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel.Security;

[TestClass]
public class SymCryptTests
{
    private static readonly string password = "TotallySecurePasswordThatNoOneWillEverGuessTM";

    [TestMethod]
    public void Encrypt_CanDecryptBackToSameValue()
    {
        byte[] plain = "Hello, encrypted world!"u8.ToArray();
        byte[] encrypted = SymCrypt.Encrypt(password, plain);

        encrypted.Should().NotBeEmpty();
        encrypted.Should().NotBeEquivalentTo(plain);

        SymCrypt.TryDecrypt(password, encrypted, out byte[] decrypted).Should().BeTrue();
        decrypted.Should().NotBeEquivalentTo(encrypted);
        decrypted.Should().BeEquivalentTo(plain);
    }

    [TestMethod]
    public void Encrypt_CanNotDecryptWithDifferentPassword()
    {
        string correctPassword = "the first password";
        byte[] plain = "A message that should be encrypted!"u8.ToArray();
        byte[] encrypted = SymCrypt.Encrypt(correctPassword, plain);

        encrypted.Should().NotBeEmpty();
        encrypted.Should().NotBeEquivalentTo(plain);

        SymCrypt.TryDecrypt("second ps", encrypted, out byte[] wrongDecrypt).Should().BeFalse();
        wrongDecrypt.Should().NotBeEquivalentTo(encrypted);
        wrongDecrypt.Should().NotBeEquivalentTo(plain);

        SymCrypt.TryDecrypt(correctPassword, encrypted, out byte[] correctDecrypt).Should().BeTrue();
        correctDecrypt.Should().NotBeEquivalentTo(wrongDecrypt);
        correctDecrypt.Should().BeEquivalentTo(plain);
    }
}
