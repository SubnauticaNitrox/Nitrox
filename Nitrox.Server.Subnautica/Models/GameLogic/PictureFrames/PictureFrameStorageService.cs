using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using Nitrox.Model.Core;
using Nitrox.Model.Server;

namespace Nitrox.Server.Subnautica.Models.GameLogic.PictureFrames;

/// <summary>
/// Manages the storing of image data for picture frames.
/// There's some rudimentary rate limiting to prevent malicious clients from spamming picture frame uploads.
/// </summary>
internal sealed class PictureFrameStorageService(IOptions<SubnauticaServerOptions> options, IOptions<ServerStartOptions> startOptions, PictureFrameKeyProvider keyProvider, ILogger<PictureFrameStorageService> logger)
{
    private const int RateLimitWindowSeconds = 10;
    private const int MaxUploadsPerWindow = 5;
    private const int MaxRequestsPerWindow = 30;

    private readonly ConcurrentDictionary<string, byte[]> cache = new();
    private readonly ConcurrentDictionary<SessionId, RateLimitWindow> uploadRateLimits = new();
    private readonly ConcurrentDictionary<SessionId, RateLimitWindow> requestRateLimits = new();

    public static string ComputeHash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    public bool TryGet(string contentHash, [NotNullWhen(true)] out byte[]? jpegBytes)
    {
        if (cache.TryGetValue(contentHash, out jpegBytes))
        {
            return true;
        }

        if (options.Value.PictureFrameSync != PictureFrameSyncMode.PERSISTED)
        {
            jpegBytes = null;
            return false;
        }

        string path = GetFilePath(contentHash);
        if (!File.Exists(path))
        {
            jpegBytes = null;
            return false;
        }

        try
        {
            jpegBytes = AesGcmFileCipher.Decrypt(File.ReadAllBytes(path), keyProvider.GetOrCreateKey());
            cache[contentHash] = jpegBytes;
            return true;
        }
        catch (CryptographicException ex)
        {
            logger.ZLogError(ex, $"Failed to decrypt persisted picture frame file for hash {contentHash}");
            jpegBytes = null;
            return false;
        }
    }
    
    public void Store(string contentHash, byte[] jpegBytes)
    {
        cache[contentHash] = jpegBytes;

        if (options.Value.PictureFrameSync != PictureFrameSyncMode.PERSISTED)
        {
            return;
        }

        string path = GetFilePath(contentHash);
        if (File.Exists(path))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, AesGcmFileCipher.Encrypt(jpegBytes, keyProvider.GetOrCreateKey()));
    }
    
    public bool TryConsumeUploadToken(SessionId sessionId) => TryConsume(uploadRateLimits, sessionId, MaxUploadsPerWindow);
    
    public bool TryConsumeRequestToken(SessionId sessionId) => TryConsume(requestRateLimits, sessionId, MaxRequestsPerWindow);

    private static bool TryConsume(ConcurrentDictionary<SessionId, RateLimitWindow> windows, SessionId sessionId, int maxPerWindow)
    {
        RateLimitWindow window = windows.GetOrAdd(sessionId, static _ => new RateLimitWindow());
        lock (window)
        {
            DateTime now = DateTime.UtcNow;
            if (now - window.WindowStart > TimeSpan.FromSeconds(RateLimitWindowSeconds))
            {
                window.WindowStart = now;
                window.Count = 0;
            }
            if (window.Count >= maxPerWindow)
            {
                return false;
            }
            window.Count++;
            return true;
        }
    }

    private string GetFilePath(string contentHash) => Path.Combine(startOptions.Value.GetServerPictureFramesPath(), $"{contentHash}.bin");

    private sealed class RateLimitWindow
    {
        public DateTime WindowStart = DateTime.UtcNow;
        public int Count;
    }
}
