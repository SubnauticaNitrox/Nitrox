using System.Collections.Concurrent;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Settings;
using UnityEngine;

namespace NitroxClient.GameLogic.PictureFrames;

/// <summary>
/// Cache of image textures keyed by content hash, and the lazy fetch-on-demand orchestration for hashes not yet in the cache.
/// </summary>
public class PictureFrameCache
{
    private readonly IPacketSender packetSender;
    private readonly ConcurrentDictionary<string, Texture2D> texturesByHash = new();
    private readonly ConcurrentDictionary<string, byte> pendingRequests = new();
    private readonly SessionByteBudget downloadBudget;

    public PictureFrameCache(IPacketSender packetSender, LocalPlayer localPlayer)
    {
        this.packetSender = packetSender;
        downloadBudget = new(() => SessionByteBudget.MbToBytes(localPlayer.PictureFrameSessionDownloadCapMbOverride ?? NitroxPrefs.PictureFrameSessionDownloadCapMb.Value));
    }

    public void Seed(string contentHash, Texture2D texture)
    {
        texturesByHash[contentHash] = texture;
    }

    public bool TryGetTexture(string contentHash, out Texture2D texture) => texturesByHash.TryGetValue(contentHash, out texture);

    public void EnsureRequested(NitroxId frameId, string contentHash)
    {
        if (texturesByHash.ContainsKey(contentHash))
        {
            return;
        }
        if (!downloadBudget.HasBudget)
        {
            if (downloadBudget.TryMarkCapReachedOnce())
            {
                Log.Warn($"Picture frame session download cap ({NitroxPrefs.PictureFrameSessionDownloadCapMb.Value} MB) reached; further picture frames will stay blank this session.");
            }
            return;
        }
        if (pendingRequests.ContainsKey(contentHash))
        {
            return;
        }
        // Sizes are unknown until a response arrives, so we have to do one at a time to stop a burst blowing the budget
        if (!pendingRequests.IsEmpty)
        {
            return;
        }
        pendingRequests.TryAdd(contentHash, 0);
        packetSender.Send(new PictureFrameDataRequest(frameId, contentHash));
    }

    public void OnResponseReceived(string contentHash, byte[] jpegBytes, bool found)
    {
        pendingRequests.TryRemove(contentHash, out _);
        if (!found || jpegBytes == null || jpegBytes.Length == 0)
        {
            return;
        }
        downloadBudget.Consume(jpegBytes.Length);

        Texture2D texture = new(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(jpegBytes, false))
        {
            UnityEngine.Object.Destroy(texture);
            return;
        }

        texturesByHash[contentHash] = texture;
    }
}
