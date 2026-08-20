using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nitrox.Model.Configuration;
using Nitrox.Model.Core;
using Nitrox.Model.Server;
using Nitrox.Server.Subnautica.Extensions;

namespace Nitrox.Server.Subnautica.Models.GameLogic.PictureFrames;

[TestClass]
public class PictureFrameStorageServiceTest
{
    private ServerStartOptions startOptions;

    [TestInitialize]
    public void TestInitialize()
    {
        startOptions = new ServerStartOptions { SaveName = $"NitroxTest_PictureFrames_{Guid.NewGuid():N}" };
    }

    [TestCleanup]
    public void TestCleanup()
    {
        string saveDir = startOptions.GetServerSavePath();
        if (Directory.Exists(saveDir))
        {
            Directory.Delete(saveDir, true);
        }
    }

    private PictureFrameStorageService CreateService(PictureFrameSyncMode mode)
    {
        SubnauticaServerOptions serverOptions = new() { PictureFrameSync = mode };
        PictureFrameKeyProvider keyProvider = new(Options.Create(startOptions));
        return new PictureFrameStorageService(Options.Create(serverOptions), Options.Create(startOptions), keyProvider, NullLogger<PictureFrameStorageService>.Instance);
    }

    [TestMethod]
    public void ComputeHash_SameBytes_ReturnsSameHash()
    {
        byte[] bytes = [.. "some picture bytes"u8];

        string hash1 = PictureFrameStorageService.ComputeHash(bytes);
        string hash2 = PictureFrameStorageService.ComputeHash(bytes);

        hash1.Should().Be(hash2);
        hash1.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void ComputeHash_DifferentBytes_ReturnsDifferentHash()
    {
        string hash1 = PictureFrameStorageService.ComputeHash([.. "picture A"u8]);
        string hash2 = PictureFrameStorageService.ComputeHash([.. "picture B"u8]);

        hash1.Should().NotBe(hash2);
    }

    [TestMethod]
    public void Session_StoreThenGet_ReturnsBytesWithoutTouchingDisk()
    {
        PictureFrameStorageService storage = CreateService(PictureFrameSyncMode.SESSION);
        byte[] bytes = [.. "session picture bytes"u8];
        string hash = PictureFrameStorageService.ComputeHash(bytes);

        storage.Store(hash, bytes);

        storage.TryGet(hash, out byte[]? result).Should().BeTrue();
        result.Should().Equal(bytes);
        Directory.Exists(startOptions.GetServerPictureFramesPath()).Should().BeFalse("SESSION mode must never write to disk");
    }

    [TestMethod]
    public void Session_UnknownHash_ReturnsNotFound()
    {
        PictureFrameStorageService storage = CreateService(PictureFrameSyncMode.SESSION);

        storage.TryGet("unknown-hash", out _).Should().BeFalse();
    }

    [TestMethod]
    public void Persisted_StoreThenGet_RoundTripsThroughDisk()
    {
        PictureFrameStorageService storage = CreateService(PictureFrameSyncMode.PERSISTED);
        byte[] bytes = [.. "persisted picture bytes"u8];
        string hash = PictureFrameStorageService.ComputeHash(bytes);

        storage.Store(hash, bytes);

        string filePath = Path.Combine(startOptions.GetServerPictureFramesPath(), $"{hash}.bin");
        File.Exists(filePath).Should().BeTrue();
        Encoding.UTF8.GetString(File.ReadAllBytes(filePath)).Should().NotContain("persisted picture bytes", "the file must be encrypted, not plaintext");
        
        PictureFrameStorageService restarted = CreateService(PictureFrameSyncMode.PERSISTED);
        restarted.TryGet(hash, out byte[]? result).Should().BeTrue();
        result.Should().Equal(bytes);
    }

    [TestMethod]
    public void Persisted_StoreSameHashTwice_DoesNotDuplicateFile()
    {
        PictureFrameStorageService storage = CreateService(PictureFrameSyncMode.PERSISTED);
        byte[] bytes = [.. "dedup picture bytes"u8];
        string hash = PictureFrameStorageService.ComputeHash(bytes);

        storage.Store(hash, bytes);
        DateTime firstWriteTimeUtc = File.GetLastWriteTimeUtc(Path.Combine(startOptions.GetServerPictureFramesPath(), $"{hash}.bin"));

        storage.Store(hash, bytes);
        DateTime secondWriteTimeUtc = File.GetLastWriteTimeUtc(Path.Combine(startOptions.GetServerPictureFramesPath(), $"{hash}.bin"));

        secondWriteTimeUtc.Should().Be(firstWriteTimeUtc, "storing the same hash again must not rewrite the file");
    }

    [TestMethod]
    public void RateLimiter_UploadTokens_AllowsUpToLimitThenRejects()
    {
        PictureFrameStorageService storage = CreateService(PictureFrameSyncMode.SESSION);
        SessionId sessionId = 1;

        int allowed = 0;
        for (int i = 0; i < 10; i++)
        {
            if (storage.TryConsumeUploadToken(sessionId))
            {
                allowed++;
            }
        }

        allowed.Should().BeLessThan(10, "the rate limiter should reject once a player exceeds the per-window upload limit");
        allowed.Should().BeGreaterThan(0);
    }

    [TestMethod]
    public void RateLimiter_DifferentSessions_TrackedIndependently()
    {
        PictureFrameStorageService storage = CreateService(PictureFrameSyncMode.SESSION);
        SessionId sessionA = 1;
        SessionId sessionB = 2;

        for (int i = 0; i < 5; i++)
        {
            storage.TryConsumeUploadToken(sessionA);
        }

        storage.TryConsumeUploadToken(sessionB).Should().BeTrue("a different player's requests must not be blocked by another player's rate limit");
    }
}
