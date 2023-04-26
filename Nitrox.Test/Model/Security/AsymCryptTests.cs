using System;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel.Security;

[TestClass]
public class AsymCryptTests
{
    private string keyFile;

    [TestInitialize]
    public void Setup()
    {
        keyFile = Path.ChangeExtension(Path.GetTempFileName(), "key");
        AsymCrypt.CreateKey(keyFile, 1024); // Using lower key size so tests aren't so slow..
    }

    [TestMethod]
    public void Encrypt_CanDecryptEncryptedInput()
    {
        byte[] input = "Hello, encrypted world!"u8.ToArray();
        Console.Write("Input: ");
        Console.WriteLine(Encoding.UTF8.GetString(input));

        byte[] encrypted = AsymCrypt.Encrypt(keyFile, input);
        encrypted.Should().NotBeEquivalentTo(input);

        Console.Write("Encrypted: ");
        Console.WriteLine(BitConverter.ToString(encrypted).Replace("-", ""));
        AsymCrypt.Decrypt(keyFile, encrypted).Should().BeEquivalentTo(input);
    }

    [TestMethod]
    public void GetPublicPemFromPrivatePem_CanDerivePublicKeyFromPrivatePem()
    {
        // Store public key in PEM file.
        string publicPem = AsymCrypt.GetPublicPemFromPrivateKeyFile(keyFile);
        publicPem.Should().NotBeEmpty();
        Console.WriteLine("Public PEM:");
        Console.Write(publicPem);
        string publicPemFile = Path.ChangeExtension(Path.GetTempFileName(), "pem");
        File.WriteAllText(publicPemFile, publicPem);

        try
        {
            // Encrypt something with the new PEM file.
            byte[] input = "Encrypted with public key that was derived from private key"u8.ToArray();
            byte[] encrypted = AsymCrypt.Encrypt(publicPemFile, input);
            encrypted.Should().NotBeEmpty();

            // Decrypt with private key (supposedly known by other party).
            string decrypted = Encoding.UTF8.GetString(AsymCrypt.Decrypt(keyFile, encrypted));
            decrypted.Should().BeEquivalentTo(Encoding.UTF8.GetString(input));
            Console.WriteLine("Decrypted:");
            Console.Write(decrypted);
        }
        finally
        {
            File.Delete(publicPemFile);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        File.Delete(keyFile);
    }
}
