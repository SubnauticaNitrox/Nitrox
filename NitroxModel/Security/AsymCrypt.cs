using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;

namespace NitroxModel.Security;

/// <summary>
///     API wrapper for BouncyCastle's asymmetric encryption algorithms. Uses PEM files to store public/private key pairs
///     that can then be used to encrypt, decrypt and sign data.
/// </summary>
public static class AsymCrypt
{
    private static readonly ConcurrentDictionary<string, PemUser> pemCache = new();

    /// <summary>
    ///     Creates a new private key file for asymmetric encryption. The public key can be derived from a private key file.
    /// </summary>
    /// <param name="keyFile">The file name for the key file.</param>
    /// <param name="keySize">The length of the private key. Should be sufficiently large to ensure security.</param>
    public static void CreateKey(string keyFile, int keySize = 4096)
    {
        RsaKeyGenerationParameters keyGenParams = new(new BigInteger("65537"), new(), keySize, 64);
        RsaKeyPairGenerator rsaKeyGen = new();
        rsaKeyGen.Init(keyGenParams);
        AsymmetricCipherKeyPair rsaKeyPair = rsaKeyGen.GenerateKeyPair();

        using StreamWriter keyStream = new(keyFile);
        using IndentedTextWriter textWriter = new(keyStream);
        using PemWriter writer = new(textWriter);
        writer.WriteObject(new Pkcs8Generator(rsaKeyPair.Private));
        writer.Writer.Flush();
    }

    /// <summary>
    ///     Gets the public key information from a private key file.
    /// </summary>
    /// <param name="keyFile">The key file with a private key.</param>
    /// <returns>
    ///     A PEM file formatted public key. Returns null if the file does not exist or the PEM file doesn't have a
    ///     private key.
    /// </returns>
    public static string GetPublicPemFromPrivateKeyFile(string keyFile)
    {
        if (!LoadPemFile(keyFile, out PemUser pem))
        {
            return null;
        }

        using StringWriter publicPemContent = new();
        using IndentedTextWriter textWriter = new(publicPemContent);
        using PemWriter writer = new(textWriter);
        writer.WriteObject(pem.PublicKey);
        writer.Writer.Flush();

        return publicPemContent.ToString();
    }

    /// <summary>
    ///     Signs the data with the private key. This allows receivers of this data, those with the matching
    ///     public key, to ensure it came from a known source.
    /// </summary>
    /// <remarks>
    ///     Important: this isn't secure (anyone with the public key can decrypt). Only to be used for signing.
    /// </remarks>
    /// <param name="keyFile">The key file that contains a private key to sign the data with.</param>
    /// <param name="data">The data to sign.</param>
    public static void Sign(string keyFile, byte[] data)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Decrypts the signed data to verify it is from the expected source.
    /// </summary>
    public static bool Verify(string pemFile, byte[] data)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Encrypts the data with the public key. Only those with the matching private key can decrypt this
    ///     data, making it secure.
    /// </summary>
    /// <param name="pemFile">The PEM file containing the public key to encrypt the data with.</param>
    /// <param name="data">The data to encrypt.</param>
    /// <exception cref="FileNotFoundException">Thrown when the PEM file does not exist.</exception>
    public static byte[] Encrypt(string pemFile, byte[] data)
    {
        if (!LoadPemFile(pemFile, out PemUser pem))
        {
            throw new FileNotFoundException("PEM file not found.", pemFile);
        }

        return pem.Encoder.Value.ProcessBlock(data, 0, data.Length);
    }

    /// <summary>
    ///     Decrypts the data with the private key. The data should have been encrypted with the matching
    ///     public key.
    /// </summary>
    /// <param name="keyFile">The key file containing the private key to decrypt the data with.</param>
    /// <param name="data">The data to decrypt.</param>
    public static byte[] Decrypt(string keyFile, byte[] data)
    {
        if (!LoadPemFile(keyFile, out PemUser pem))
        {
            throw new FileNotFoundException("Key file not found.", keyFile);
        }
        if (!pem.CanDecrypt)
        {
            throw new Exception($"The key file '{keyFile}' does not contain a private key, decryption is unavailable.");
        }

        return pem.Decoder.Value.ProcessBlock(data, 0, data.Length);
    }

    public static void Invalidate()
    {
        pemCache.Clear();
    }

    /// <summary>
    ///     Loads a file that can either contain a private key (usually .key) or public key (usually .pem).
    ///     Multi-content PEM data is not supported.
    /// </summary>
    private static bool LoadPemFile(string pemFile, out PemUser pemUser)
    {
        pemFile = Path.GetFullPath(pemFile);
        if (!pemCache.ContainsKey(pemFile))
        {
            if (!File.Exists(pemFile))
            {
                pemUser = default;
                return false;
            }

            pemUser = pemCache[pemFile] = new PemUser(pemFile);
            return true;
        }

        pemUser = pemCache[pemFile];
        return true;
    }

    private readonly record struct PemUser
    {
        public Lazy<OaepEncoding> Encoder { get; }
        public Lazy<OaepEncoding> Decoder { get; }

        public bool CanDecrypt => Decoder != null;

        public RsaPrivateCrtKeyParameters PrivateKey { get; }
        public RsaKeyParameters PublicKey { get; }

        public PemUser(string pemFile)
        {
            string pemContent = File.ReadAllText(pemFile);
            if (string.IsNullOrWhiteSpace(pemContent))
            {
                throw new Exception($"Invalid PEM data in file '{pemFile}'");
            }

            using StringReader stringReader = new(pemContent);
            using PemReader reader = new(stringReader);
            object pemObject = reader.ReadObject();
            switch (pemObject)
            {
                case RsaPrivateCrtKeyParameters privatePem:
                    PrivateKey = privatePem;
                    PublicKey = new(false, privatePem.Modulus, privatePem.PublicExponent);
                    break;
                case RsaKeyParameters publicPem:
                    PublicKey = publicPem;
                    break;
                default:
                    throw new NotSupportedException($"Unsupported PEM file '{pemFile}'");
            }

            RsaKeyParameters publicKey = PublicKey;
            Encoder = new Lazy<OaepEncoding>(() =>
            {
                OaepEncoding encoder = CreateEncoder();
                encoder.Init(true, publicKey);
                return encoder;
            });
            RsaKeyParameters privateKey = PrivateKey;
            Decoder = new Lazy<OaepEncoding>(() =>
            {
                OaepEncoding encoder = CreateEncoder();
                encoder.Init(false, privateKey);
                return encoder;
            });
        }

        private static OaepEncoding CreateEncoder()
        {
            return new(new RsaEngine(), new Sha256Digest(), new Sha256Digest(), null);
        }
    }
}
