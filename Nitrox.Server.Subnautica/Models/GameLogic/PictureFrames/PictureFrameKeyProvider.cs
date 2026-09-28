using System.IO;
using System.Security.Cryptography;

namespace Nitrox.Server.Subnautica.Models.GameLogic.PictureFrames;

/// <summary>
/// Generates and loads the AES-256 key used to encrypt persisted picture frame files on disk.
/// </summary>
internal sealed class PictureFrameKeyProvider(IOptions<ServerStartOptions> startOptions)
{
    private const int KeySizeBytes = 32; // AES-256

    private readonly object keyLock = new();
    private byte[]? key;

    public byte[] GetOrCreateKey()
    {
        if (key != null)
        {
            return key;
        }

        lock (keyLock)
        {
            if (key != null)
            {
                return key;
            }

            string keyPath = startOptions.Value.GetServerPictureFrameKeyPath();
            if (File.Exists(keyPath))
            {
                byte[] loaded = File.ReadAllBytes(keyPath);
                if (loaded.Length == KeySizeBytes)
                {
                    key = loaded;
                    return key;
                }
            }

            byte[] generated = RandomNumberGenerator.GetBytes(KeySizeBytes);
            Directory.CreateDirectory(Path.GetDirectoryName(keyPath)!);
            File.WriteAllBytes(keyPath, generated);
            key = generated;
            return key;
        }
    }
}
